using System;
using System.Diagnostics;
using System.Linq;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using pzd.lib.exts;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class EditorUtils {
    [Conditional("UNITY_EDITOR")]
    public static void recordEditorChanges(this Object o, string name) {
#if UNITY_EDITOR
      Undo.RecordObject(o, name);
      EditorUtility.SetDirty(o);
#endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void recordEditorChanges(this Object[] objects, string name) {
#if UNITY_EDITOR
      Undo.RecordObjects(objects, name);
      foreach (var o in objects) {
        EditorUtility.SetDirty(o);
      }
#endif
    }

    public static bool inBatchMode =>
#if UNITY_EDITOR
      InternalEditorUtility.inBatchMode
#else
      false
#endif
      ;

    [PublicAPI]
    public static void userInfo(
      string title, string body, Log.Level level = Log.Level.INFO, Object context = null
    ) {
      var log = Log.@default;
      if (log.willLog(level)) log.log(
        level,
        LogEntry.simple(
          $"########## {title} ##########\n\n" +
          $"{body}\n\n" +
          $"############################################################",
          context: context
        )
      );
#if UNITY_EDITOR
      const int lineCount = 50;
      var lines = body.Split('\n');
      if (lines.Length > lineCount) body = $"{lines.Take(lineCount).mkString('\n')}\n... [Full message in logs]";
      if (!InternalEditorUtility.inBatchMode) EditorUtility.DisplayDialog(title, body, "OK");
#endif
    }

    [PublicAPI]
    public static Exception userException(string title, string body) {
      userInfo(title, body, Log.Level.ERROR);
      return new Exception("Aborting.");
    }

#if UNITY_EDITOR
    public enum DisplayDialogResult : { OK, Alt, Cancel }
    public static DisplayDialogResult displayDialogComplex(
      string title, string message, string ok, string cancel, string alt
    ) {
      // Alt and cancel is mixed intentionally
      // Unity maps 'x button' and 'keyboard esc' to alt and not to cancel for some reason
      var result = EditorUtility.DisplayDialogComplex(
        title: title, message: message, ok: ok, cancel: alt, alt: cancel
      );
      switch (result) {
        case 0: return DisplayDialogResult.OK;
        case 1: return DisplayDialogResult.Alt;
        case 2: return DisplayDialogResult.Cancel;
        default: throw new ArgumentOutOfRangeException($"Unknown return value from unity: {result}");
      }
    }
#endif
  }
}