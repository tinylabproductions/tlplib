using com.tinylabproductions.TLPLib.Editor.extensions;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.unity_serialization;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {

  [CustomPropertyDrawer(typeof(UnityEither), useForChildren: true), CanEditMultipleObjects]
  public class UnityEitherDrawer : PropertyDrawer {

    static SerializedProperty getAProp(SerializedProperty property, string propName) =>
      property.FindPropertyRelative(propName);

    static Option<SerializedProperty> getValueProp(SerializedProperty property, string propName) =>
      property.FindPropertyRelative(propName).opt();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return base.GetPropertyHeight(property, label) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
      OnGUI_(position, property, label, "_isA", "a", "b");

    public static void OnGUI_(
      Rect position, SerializedProperty property, GUIContent label,
      string isAPropertyName, string leftValuePropertyName, string rightValuePropertyName
    ) {

      EditorGUI.BeginProperty(position, label, property);

      Rect checkboxRect, valueRect;
      if (label.text == "" && label.image == null) {
        const float TOGGLE_WIDTH = 16;
        const float FIELD_HEIGHT = 16;
        checkboxRect = new Rect(position.x, position.y, TOGGLE_WIDTH, FIELD_HEIGHT);
        valueRect = new Rect(position.x, position.y + 16, position.width, FIELD_HEIGHT);
      }
      else {
        DrawerUtils.twoFieldsLabel(EditorGUI.IndentedRect(position), out checkboxRect, out valueRect);
      }

      var maybeIsAProp = getAProp(property, isAPropertyName);

      void drawProperty(SerializedProperty valueProp) {
        EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
        if (valueProp.propertyType == SerializedPropertyType.Generic) {
          using (new EditorIndent(EditorGUI.indentLevel + 1)) {
            EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
            EditorGUILayout.PropertyField(valueProp, includeChildren: true);
          }
        }
        else {
          var typeLabel = new GUIContent(valueProp.type);
          EditorGUIUtility.labelWidth = Mathf.Clamp(valueProp.type.Length * 7.5f, 0, position.width / 2);
          EditorGUI.PropertyField(valueRect, valueProp, typeLabel);
        }
      }

      var maybeLeftValueProp = getValueProp(property, leftValuePropertyName);
      var maybeRightValueProp = getValueProp(property, rightValuePropertyName);

      EditorGUI.BeginChangeCheck();
      EditorGUI.showMixedValue = maybeIsAProp.hasMultipleDifferentValues;
      var isLeft = EditorGUI.ToggleLeft(checkboxRect, "isLeft", maybeIsAProp.boolValue);
      var isLeftChanged = EditorGUI.EndChangeCheck();
      if (isLeftChanged) maybeIsAProp.boolValue = isLeft;

      if (isLeft) {
        if (maybeLeftValueProp.valueOut(out var leftValueProp)) {
          drawProperty(leftValueProp);
        }
        else {
          if (isLeftChanged) leftValueProp.setToDefaultValue();
        }
      }
      else {
        if (maybeRightValueProp.valueOut(out var rightValueProp)) {
          drawProperty(rightValueProp);
        }
        else {
          if (isLeftChanged) rightValueProp.setToDefaultValue();
        }
      }

      EditorGUI.EndProperty();
    }
  }
}