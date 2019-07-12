using System;
using System.IO;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using pzd.lib.exts;
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
    if (movedAssets.Length > 0 && Log.d.isDebug()) Log.d.debug(
      $"{nameof(DateModifiedUpdater)}.{nameof(OnPostprocessAllAssets)}[\n" +
      $"  {nameof(movedAssets)}: {movedAssets.mkStringEnum()}\n" +
      $"]"
    );
    foreach (var relativePath in movedAssets) {
      File.SetLastWriteTime(relativePath, DateTime.Now);
      File.SetLastWriteTime($"{relativePath}.meta", DateTime.Now);
    }
  }
}