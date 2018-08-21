using JetBrains.Annotations;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  public static class SerializedObjectExts {
    [PublicAPI]
    public static void drawInspector(this SerializedObject obj, bool onlyVisible = true) {
      var prop = obj.GetIterator();
      // enter first child - unity returns an iterator to empty ('') property path, which represents
      // object root, but this property is invalid on itself.
      prop.next(enterChildren: true, onlyVisible: onlyVisible);

      while (prop.next(enterChildren: false, onlyVisible: onlyVisible)) {
        EditorGUILayout.PropertyField(prop, includeChildren: true);
      }

      obj.ApplyModifiedProperties();
    }
  }
}