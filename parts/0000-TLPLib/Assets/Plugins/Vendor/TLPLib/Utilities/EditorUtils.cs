using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
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
      UnityEditorInternal.InternalEditorUtility.inBatchMode
#else
      false
#endif
      ;

    public static void userInfo(string title, string body, Log.Level level = Log.Level.INFO) {
      var log = Log.defaultLogger;
      if (log.willLog(level)) log.log(level, $"{title}\n\n{body}");
#if UNITY_EDITOR
      EditorUtility.DisplayDialog(title, body, "OK");
#endif
    }
  }
}