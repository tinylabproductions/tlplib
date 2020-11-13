using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Editor.Utils;
using JetBrains.Annotations;
using pzd.lib.collection;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  /// <summary>
  /// Sometimes Unity fails to import scenes or prefabs. This tries to detect that.
  /// </summary>
  [PublicAPI] public static class AssetFilesValidator {
    /// <summary>
    /// Checks whether Unity has correctly imported all scenes and prefabs.
    /// </summary>
    /// <param name="validateScenes">Should we check scenes?</param>
    /// <param name="validatePrefabs">Should we check prefabs?</param>
    /// <param name="showProgress">Should editor progress be shown?</param>
    public static IEnumerable<ObjectValidator.Error> validateAll(
      bool validateScenes, bool validatePrefabs, bool showProgress
    ) =>
      validate(
        scenePaths:
          validateScenes
            ? Directory.EnumerateFiles("Assets", "*.unity", SearchOption.AllDirectories).ToArray()
            : EmptyArray<string>._,
        prefabPaths:
          validatePrefabs
            ? Directory.EnumerateFiles("Assets", "*.prefab", SearchOption.AllDirectories).ToArray()
            : EmptyArray<string>._,
        showProgress: showProgress
      );

    /// <summary>
    /// Checks whether Unity has correctly imported scenes and prefabs at given paths. 
    /// </summary>
    /// <param name="scenePaths">Paths to the scene files (ending in .unity)</param>
    /// <param name="prefabPaths">Paths to the prefab files (ending in .prefab)</param>
    /// <param name="showProgress">Should editor progress be shown?</param>
    public static IEnumerable<ObjectValidator.Error> validate(
      ICollection<string> scenePaths, ICollection<string> prefabPaths, bool showProgress
    ) {
      (string path, Object obj)[] badScenes, badPrefabs;
      {
        var maybeProgress = showProgress ? new EditorProgress("Asset files validator") : null;
        try {
          maybeProgress?.start("Checking scene assets");
          badScenes = scenePaths.collect((path, idx) => {
            maybeProgress?.progress(idx, scenePaths.Count);
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            return obj is SceneAsset ? None._ : Some.a((path, obj));
          }).ToArray();
          maybeProgress?.start("Checking prefab assets");
          badPrefabs = prefabPaths.collect((path, idx) => {
            maybeProgress?.progress(idx, prefabPaths.Count);
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            return obj is GameObject ? None._ : Some.a((path, obj));
          }).ToArray();
        }
        finally {
          maybeProgress?.Dispose();
        }
      }

      foreach (var error in createErrors(badScenes, "scene")) yield return error;
      foreach (var error in createErrors(badPrefabs, "prefab")) yield return error;

      IEnumerable<ObjectValidator.Error> createErrors(
        IEnumerable<(string path, Object obj)> src, string name
      ) {
        foreach (var (path, obj) in src) {
          var objStr = obj ? obj.GetType().FullName : "null";
          yield return new ObjectValidator.Error(
            ObjectValidator.Error.Type.AssetCorrupted,
            $"Expected file to be a {name}, but it was {objStr}",
            obj,
            objFullPath: $"{name} asset import failed",
            location: new AssetPath(path)
          );
        }
      }
    }
  }
}