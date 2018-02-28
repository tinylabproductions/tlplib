#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
namespace com.tinylabproductions.TLPLib.Extensions {
  public static class SerializedObjectExts {
    public static IEnumerable<SerializedProperty> iterate(
      this SerializedObject so, bool enterChildren
    ) {
      var sp = so.GetIterator();
      /*
       * It is mandatory to pass 'true' on the first call.
       * Otherwise you get an error:
       * 'Invalid iteration - (You need to call Next (true) on the first element to get to the first element)'
       */
      if (sp.Next(true)) {
        yield return sp;
        while (sp.Next(enterChildren)) yield return sp;
      }
    }

    public static IEnumerable<SerializedProperty> iterateVisible(
      this SerializedObject so, bool enterChildren
    ) {
      var sp = so.GetIterator();
      /*
       * It is mandatory to pass 'true' on the first call.
       * Otherwise you get an error:
       * 'Invalid iteration - (You need to call Next (true) on the first element to get to the first element)'
       */
      if (sp.NextVisible(true)) {
        yield return sp;
        while (sp.NextVisible(enterChildren)) yield return sp;
      }
    }
  }
}
#endif
