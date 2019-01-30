using com.tinylabproductions.TLPLib.Editor.extensions;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.unity_serialization;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  [CustomPropertyDrawer(typeof(UnityOption), useForChildren: true), CanEditMultipleObjects]
  public class UnityOptionDrawer : PropertyDrawer {
    static SerializedProperty getSomeProp(SerializedProperty property, string propName) =>
      property.FindPropertyRelative(propName);

    static Option<SerializedProperty> getValueProp(SerializedProperty property, string propName) =>
      property.FindPropertyRelative(propName).opt();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
      OnGUI_(position, property, label, "_isSome", "_value");

    public static void OnGUI_(
      Rect position, SerializedProperty property, GUIContent label,
      string somePropertyName, string valuePropertyName
    ) {
      EditorGUI.BeginProperty(position, label, property);

      Rect firstRect, secondRect;
      if (label.text == "" && label.image == null) {
        const float TOGGLE_WIDTH = 16;
        firstRect = new Rect(position.x, position.y, TOGGLE_WIDTH, position.height);
        secondRect = new Rect(position.x + TOGGLE_WIDTH, position.y, position.width - TOGGLE_WIDTH, position.height);
      }
      else {
        DrawerUtils.twoFieldsLabel(position, out firstRect, out secondRect);
      }

      var isSomeProp = getSomeProp(property, somePropertyName);
      var maybeValueProp = getValueProp(property, valuePropertyName);

      EditorGUI.BeginChangeCheck();
      EditorGUI.showMixedValue = isSomeProp.hasMultipleDifferentValues;
      var isSome = EditorGUI.ToggleLeft(firstRect, label, isSomeProp.boolValue);
      var someChanged = EditorGUI.EndChangeCheck();
      if (someChanged) isSomeProp.boolValue = isSome;
      if (maybeValueProp.valueOut(out var valueProp)) {
        if (isSome) {
          EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
          if (valueProp.propertyType == SerializedPropertyType.Generic) {
            using (EditorIndent.plus()) {
              EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
              EditorGUILayout.PropertyField(valueProp, includeChildren: true);
            }
          }
          else {
            EditorGUI.PropertyField(secondRect, valueProp, GUIContent.none);
          }
        }
        else {
          if (someChanged) valueProp.setToDefaultValue();
        }
      }
      else {
        EditorGUI.LabelField(secondRect, "type not serializable!");
      }

      EditorGUI.EndProperty();
    }
  }
}