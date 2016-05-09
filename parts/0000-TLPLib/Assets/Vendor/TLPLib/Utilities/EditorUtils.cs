using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class EditorUtils {
    [Conditional("UNITY_EDITOR")]
    public static void recordEditorChanges(this Object o, string name) {
      Undo.RecordObject(o, name);
      EditorUtility.SetDirty(o);
    }
  }
}