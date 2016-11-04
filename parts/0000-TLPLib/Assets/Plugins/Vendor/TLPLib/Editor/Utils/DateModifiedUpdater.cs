using System;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;

/** 
 * Git identifies file changes by file size and date modified
 * If you swap the names of 2 files git doesn't see the changes in meta files
 * This code updates 'Date modified' so git detects the changes
*/
public class DateModifiedUpdater : AssetPostprocessor {
  [UsedImplicitly]
  static void OnPostprocessAllAssets(
    string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths
  ) {
    foreach (var relativePath in movedAssets) {
      File.SetLastWriteTime(relativePath, DateTime.Now);
      File.SetLastWriteTime($"{relativePath}.meta", DateTime.Now);
    }
  }
}