using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class EditorUtils {
    public static void recordEditorChanges(this Object o, string name) {
#if UNITY_EDITOR
      Undo.RecordObject(o, name);
      EditorUtility.SetDirty(o);
#endif
    }

    public static bool inBatchMode =>
#if UNITY_EDITOR
      InternalEditorUtility.inBatchMode
#else
      false
#endif
      ;

    public static void userInfo(string title, string body, Log.Level level = Log.Level.INFO) {
      var log = Log.@default;
      if (log.willLog(level)) log.log(
        level,
        LogEntry.simple(
          $"########## {title} ##########\n\n" +
          $"{body}\n\n" +
          $"############################################################"
        )
      );
#if UNITY_EDITOR
      const int lineCount = 50;
      var lines = body.Split('\n');
      if (lines.Length > lineCount) body = $"{lines.Take(lineCount).mkString('\n')}\n... [Full message in logs]";
      if (!InternalEditorUtility.inBatchMode) EditorUtility.DisplayDialog(title, body, "OK");
#endif
    }
  }
}