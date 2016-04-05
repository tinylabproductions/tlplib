using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class EditorUtils {
    public static IEnumerable<GameObject> getSceneObjects() {
      return Resources.FindObjectsOfTypeAll<GameObject>()
          .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                 && go.hideFlags == HideFlags.None);
    }

    // http://forum.unity3d.com/threads/how-to-get-the-local-identifier-in-file-for-scene-objects.265686/
    public static int getFileID(Object unityObject) {
      var inspectorModeInfo =
        typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

      var serializedObject = new SerializedObject(unityObject);
      inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

      var localIdProp =
          serializedObject.FindProperty("m_LocalIdentfierInFile");   //note the misspelling!

      return localIdProp.intValue;
    }
  }
}