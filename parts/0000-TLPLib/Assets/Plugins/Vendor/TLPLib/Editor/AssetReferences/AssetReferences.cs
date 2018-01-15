using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.AssetReferences {
  public class AssetReferences {
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
    public ImmutableList<string> findParentScenes(string guid) {
      // TODO: expensive operation. Need to cache results
      // Dijkstra
      var visited = new HashSet<string>();
      var q = new Queue<string>();
      q.Enqueue(guid);
      var res = ImmutableList.CreateBuilder<string>();
      while (q.Count > 0) {
        var cur = q.Dequeue();
        if (visited.Contains(cur)) continue;
        visited.Add(cur);
        var path = AssetDatabase.GUIDToAssetPath(cur);
        if (path.EndsWithFast(".unity")) {
          res.Add(cur);
        }
        if (parents.ContainsKey(cur)) {
          foreach (var parent in parents[cur]) {
            if (!visited.Contains(parent)) q.Enqueue(parent);
          }
        }
      }
      return res.ToImmutable();
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
        || p.EndsWithFast(".mat");

      var assets = data.filter(predicate, _ => predicate(_.fromPath));
      var firstScan = children.isEmpty() && parents.isEmpty();
      var updatedChildren =
        firstScan
        ? children
        : new Dictionary<string, HashSet<string>>();

      var progressIdx = 0;
      Action updateProgress = () => progress.value = (float)progressIdx / assets.totalAssets;

      var addedPool = new PCQueue(workers);
      foreach (var added in assets.newAssets) {
        updateProgress();
        addedPool.EnqueueItem(() => {
          parseFile(pathToGuid, log, added, updatedChildren);
          lock (data) {
            progressIdx++;
            updateProgress();
          }
        });
      }
      // Block until parallel part is done.
      addedPool.Shutdown(waitForWorkers: true);

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