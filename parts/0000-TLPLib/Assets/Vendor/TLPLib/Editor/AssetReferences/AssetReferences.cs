using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using com.tinylabproductions.TLPLib.Data;
using pzd.lib.log;
using GenerationAttributes;
using pzd.lib.data;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.AssetReferences {
  public partial class AssetReferences {
    public readonly Dictionary<string, HashSet<string>> parents =
      new Dictionary<string, HashSet<string>>();
    public readonly Dictionary<string, HashSet<string>> children =
      new Dictionary<string, HashSet<string>>();
    readonly Dictionary<string, string> pathToGuid =
      new Dictionary<string, string>();

    AssetReferences() {}

    public static AssetReferences a(
      AssetUpdate data, int workers, Ref<float> progress, ILog log
    ) {
      var refs = new AssetReferences();
      process(
        data, workers,
        pathToGuid: refs.pathToGuid, parents: refs.parents, children: refs.children,
        progress: progress, log: log
      );
      return refs;
    }

    public static readonly Regex
      // GUID regex for matching guids in meta files
      metaGuid = new Regex(@"guid: (\w+)"),
      // GUID regex for matching guids in other files
      guidRegex = new Regex(
        @"{fileID:\s+(\d+),\s+guid:\s+(\w+),\s+type:\s+\d+}",
        RegexOptions.Compiled | RegexOptions.Multiline
      );

    public void update(AssetUpdate data, int workers, Ref<float> progress, ILog log) {
      process(
        data, workers,
        pathToGuid: pathToGuid, parents: parents, children: children,
        progress: progress, log: log
      );
    }

    /// <summary>
    /// Given an object GUID find all scenes where that particular GUID is being used.
    /// </summary>
    /// <returns>guids for scenes where given guid is used</returns>
    public ImmutableList<Chain> findParentScenes(string guid) => 
      findParentX(guid, path => path.EndsWithFast(".unity"));
    
    /// <summary>
    /// Given an object GUID find all resources where that particular GUID is being used.
    /// </summary>
    /// <returns>guids for resources where given guid is used</returns>
    public ImmutableList<Chain> findParentResources(string guid) => 
      findParentX(guid, path => path.ToLowerInvariant().Contains("/resources/"));

    [Record] public sealed partial class Chain {
      public readonly NonEmpty<ImmutableList<string>> guids;

      public string mainGuid => guids.head();
    }
    
    public ImmutableList<Chain> findParentX(string guid, Func<string, bool> pathPredicate) {
      // TODO: expensive operation. Need to cache results
      // Dijkstra
      
      // guid -> child guid
      var visited = new Dictionary<string, Option<string>>();
      var q = new Queue<(string current, Option<string> child)>();
      q.Enqueue((guid, Option<string>.None));
      var res = ImmutableList.CreateBuilder<Chain>();
      while (q.Count > 0) {
        var (current, maybeChild) = q.Dequeue();
        if (visited.ContainsKey(current)) continue;
        visited.Add(current, maybeChild);
        var path = AssetDatabase.GUIDToAssetPath(current);
        if (pathPredicate(path)) {
          res.Add(makeChain(current));
        }
        if (parents.TryGetValue(current, out var currentParents)) {
          foreach (var parent in currentParents) {
            if (!visited.ContainsKey(parent)) q.Enqueue((parent, Some.a(current)));
          }
        }
      }
      return res.ToImmutable();

      Chain makeChain(string g) {
        // head points to parent, last points to the object from which we started the search
        var builder = ImmutableList.CreateBuilder<string>();
        builder.Add(g);
        var current = g;
        while (visited.TryGetValue(current, out var maybeChild) && maybeChild.valueOut(out var child)) {
          builder.Add(child);
          current = child;
        }

        return new Chain(builder.ToImmutable().toNonEmpty().get);
      }
    }

    static void process(
      AssetUpdate data, int workers,
      Dictionary<string, string> pathToGuid,
      Dictionary<string, HashSet<string>> parents,
      Dictionary<string, HashSet<string>> children,
      Ref<float> progress, ILog log
    ) {
      progress.value = 0;
      Func<string, bool> predicate = p =>
        p.EndsWithFast(".asset")
        || p.EndsWithFast(".prefab")
        || p.EndsWithFast(".unity")
        || p.EndsWithFast(".mat")
        || p.EndsWithFast(".spriteatlas");

      var assets = data.filter(predicate, _ => predicate(_.fromPath));
      var firstScan = children.isEmpty() && parents.isEmpty();
      var updatedChildren =
        firstScan
        ? children
        : new Dictionary<string, HashSet<string>>();

      var progressIdx = 0;
      Action updateProgress = () => progress.value = (float)progressIdx / assets.totalAssets;

      Parallel.ForEach(assets.newAssets, added => {
        parseFile(pathToGuid, log, added, updatedChildren);
        lock (data) {
          progressIdx++;
          updateProgress();
        }
      });

      foreach (var deleted in assets.deletedAssets) {
        updateProgress();
        if (pathToGuid.ContainsKey(deleted)) {
          updatedChildren.getOrUpdate(pathToGuid[deleted], () => new HashSet<string>());
          pathToGuid.Remove(deleted);
        }
        progressIdx++;
      }
      foreach (var moved in assets.movedAssets) {
        updateProgress();
        foreach (var guid in pathToGuid.getAndRemove(moved.fromPath))
          pathToGuid[moved.toPath] = guid;
        progressIdx++;
      }
      updateProgress();

      if (!firstScan) {
        foreach (var parent in updatedChildren.Keys) {
          if (children.ContainsKey(parent)) {
            var set = children[parent];
            foreach (var child in set) parents[child].Remove(parent);
            children.Remove(parent);
          }
        }
      }
      addParents(parents, updatedChildren);
      if (!firstScan) {
        foreach (var kv in updatedChildren) {
          if (kv.Value.Count > 0) children.Add(kv.Key, kv.Value);
        }
      }
    }

    static void parseFile(
      Dictionary<string, string> pathToGuid, ILog log,
      string assetPath,
      Dictionary<string, HashSet<string>> updatedChildren
    ) {
      string guid;
      if (!getGuid(assetPath, out guid, log)) return;

      lock(pathToGuid) { pathToGuid[assetPath] = guid; }
      var bytes = File.ReadAllBytes(assetPath);
      var str = Encoding.ASCII.GetString(bytes);
      var m = guidRegex.Match(str);
      while (m.Success) {
        var childGuid = m.Groups[2].Value;
        lock (updatedChildren) {
          updatedChildren.getOrUpdate(guid, () => new HashSet<string>()).Add(childGuid);
        }
        m = m.NextMatch();
      }
    }

    static void addParents(
      Dictionary<string, HashSet<string>> parents,
      Dictionary<string, HashSet<string>> updatedChildren
    ) {
      foreach (var kv in updatedChildren) {
        var parent = kv.Key;
        foreach (var child in kv.Value) {
          parents.getOrUpdate(child, () => new HashSet<string>()).Add(parent);
        }
      }
    }

    // out instead of Either for performance reasons.
    public static bool getGuid(string assetPath, out string guid, ILog log) {
      if (assetPath.StartsWithFast("ProjectSettings/")) {
        guid = null;
        return false;
      }

      var metaPath = $"{assetPath}.meta";
      if (File.Exists(metaPath)) {
        var fileContents = Encoding.ASCII.GetString(File.ReadAllBytes(metaPath));
        var m = metaGuid.Match(fileContents);
        if (m.Success) {
          guid = m.Groups[1].Value;
          return true;
        }
        else {
          log.error($"Guid not found for: {assetPath}");
          guid = null;
          return false;
        }
      }
      else {
        log.error($"Meta file not found for: {assetPath}");
        guid = null;
        return false;
      }
    }
  }

}