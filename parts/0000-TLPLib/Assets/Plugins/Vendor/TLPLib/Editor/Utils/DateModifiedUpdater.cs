using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;

public class DateModifiedUpdater : AssetPostprocessor {
  [UsedImplicitly]
  static void OnPostprocessAllAssets(
    string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths
  ) {
    foreach (var relativePath in movedAssets) {
      // Substring is needed to remove "Assets" from the start of the path
      File.SetLastWriteTime(relativePath, DateTime.Now);
      File.SetLastWriteTime($"{relativePath}.meta", DateTime.Now);
    }
  }
}