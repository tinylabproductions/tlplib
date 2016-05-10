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
  }
}