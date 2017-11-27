using System.Collections.Generic;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class SerializedObjectExts {
    public static IEnumerable<SerializedProperty> iterate(
      this SerializedObject so, bool enterChildren
    ) {
      var sp = so.GetIterator();
      while (sp.Next(enterChildren)) yield return sp;
    }

    public static IEnumerable<SerializedProperty> iterateVisible(
      this SerializedObject so, bool enterChildren
    ) {
      var sp = so.GetIterator();
      while (sp.NextVisible(enterChildren)) yield return sp;
    }
  }
}
