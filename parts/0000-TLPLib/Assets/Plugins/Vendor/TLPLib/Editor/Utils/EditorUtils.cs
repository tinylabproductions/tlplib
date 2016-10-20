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

    static PropertyInfo cachedInspectorModeInfo;

    // https://gist.github.com/DmitriyYukhanov/feb182871a14e355ac38
    public static long getFileID(Object unityObject) {
      var id = -1L;
      if (unityObject == null) return id;

      if (cachedInspectorModeInfo == null)
        cachedInspectorModeInfo = typeof(SerializedObject).GetProperty(
          "inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance
        );

      var serializedObject = new SerializedObject(unityObject);
      cachedInspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);
      var serializedProperty = serializedObject.FindProperty("m_LocalIdentfierInFile");
			id = serializedProperty.longValue;
      if (id <= 0) {
        var prefabType = PrefabUtility.GetPrefabType(unityObject);
        id = prefabType != PrefabType.None 
          ? getFileID(PrefabUtility.GetPrefabObject(unityObject))
          // this will work for the new objects in scene which weren't saved yet
          : unityObject.GetInstanceID();
      }

      return id;
    }
  }
}