using System;
using System.IO;
using UnityEngine;
using UnityEditor;
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable SuggestBaseTypeForParameter

public class DateModifiedUpdater : AssetPostprocessor {
  // ReSharper disable once UnusedMember.Local
  static void OnPostprocessAllAssets(
    string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths
  ) {
    foreach (var t in movedAssets) {
      var filePath = Application.dataPath + t.Substring(6);
      File.SetLastWriteTime(filePath, DateTime.Now);
      File.SetLastWriteTime($"{filePath}.meta", DateTime.Now);
    }
  }
}