using com.tinylabproductions.TLPLib.Data;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  [CustomPropertyDrawer(typeof(TagPrefab), useForChildren: true), CanEditMultipleObjects]
  public class TagPrefabDrawer : PropertyDrawer {
    static readonly GUIStyle prefabLabelStyle = new GUIStyle {
      fontStyle = FontStyle.Bold,
      fontSize = 8,
      alignment = TextAnchor.UpperCenter
    };
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
      OnGUI_(position, property, label);

    public static void OnGUI_(
      Rect position, SerializedProperty property, GUIContent label
    ) {
      EditorGUI.BeginProperty(position, label, property);

      var prop = property.FindPropertyRelative("_prefab");
      var color =
        prop.objectReferenceValue == null
          ? new Color32(211, 167, 167, 255)
          : new Color32(167, 183, 211, 255);
      EditorGUI.DrawRect(position, color);
      EditorGUI.PropertyField(position, prop, label: label, includeChildren: true);
      // Maybe draw a prefab icon later.
//      var prefabLabelPosition = position;
//      prefabLabelPosition.xMin = position.xMin - 10;
//      prefabLabelPosition.xMax = position.xMin;
//      prefabLabelPosition.yMin += 3;
//      EditorGUI.LabelField(prefabLabelPosition, "[P]", prefabLabelStyle);

      EditorGUI.EndProperty();
    }
  }
}