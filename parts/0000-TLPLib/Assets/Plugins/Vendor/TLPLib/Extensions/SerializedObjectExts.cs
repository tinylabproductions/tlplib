#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
namespace com.tinylabproductions.TLPLib.Extensions {
  public static class SerializedObjectExts {
    public static IEnumerable<SerializedProperty> iterate(
      this SerializedObject so, bool enterChildren
    ) {
      var sp = so.GetIterator();
      // It is mandatory to pass 'true' on the first call.
      if (sp.Next(true)) {
        yield return sp;
        while (sp.Next(enterChildren)) yield return sp;
      }
    }

    public static IEnumerable<SerializedProperty> iterateVisible(
      this SerializedObject so, bool enterChildren
    ) {
      var sp = so.GetIterator();
      if (sp.Next(true)) {
        yield return sp;
        while (sp.NextVisible(enterChildren)) yield return sp;
      }
    }
  }
}
#endif
