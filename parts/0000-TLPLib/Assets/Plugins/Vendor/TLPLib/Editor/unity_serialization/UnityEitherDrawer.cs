using AdvancedInspector;
using com.tinylabproductions.TLPLib.Editor.extensions;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.unity_serialization;
using UnityEditor;
using UnityEngine;
using static com.tinylabproductions.TLPLib.unity_serialization.SerializedPropertyUtils;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {

  //[CustomPropertyDrawer(typeof(UnityEither), useForChildren: true), CanEditMultipleObjects]
  public class UnityEitherDrawer : PropertyDrawer {
    
    readonly string[] popupOptions = new string[2];

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
      OnGUI_(position, property, label, popupOptions, "_isA", "a", "b");

    public static void OnGUI_(
      Rect position, SerializedProperty property, GUIContent label, string[] popupOptions,
      string isAPropertyName, string leftValuePropertyName, string rightValuePropertyName
    ) {

      var maybeIsAProp = getValuePropRelative(property, isAPropertyName);

      if (maybeIsAProp.valueOut(out var isAProp)) {

        EditorGUI.BeginProperty(position, label, property);
        
        const int GAP_WIDTH = 5;
        var popupWidth = position.width / 1.8f;
        
        var popupRect = new Rect(position.x, position.y, popupWidth, position.height);
        var valueRect = new Rect(position.x + popupWidth + GAP_WIDTH,
          position.y, position.width - (popupWidth + GAP_WIDTH), position.height);

        void drawProperty(SerializedProperty valueProp) {
          EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
          if (valueProp.propertyType == SerializedPropertyType.Generic) {
            using (new EditorIndent(EditorGUI.indentLevel + 1)) {
              //TODO write your own property field function which forwards to certain drawer to AI or to custom
              EditorGUILayout.PropertyField(valueProp, includeChildren: true);
              InspectorField aa = new InspectorField(valueProp.serializedObject.targetObjects, leftValuePropertyName);
              AdvancedInspectorControl.DrawField(aa, );
            }
          }
          else {
            EditorGUI.PropertyField(valueRect, valueProp, new GUIContent(""));
          }
        }

        if ( getValuePropRelative(property, leftValuePropertyName).valueOut(out var leftValueProp)
          && getValuePropRelative(property, rightValuePropertyName).valueOut(out var rightValueProp)
          ) {
          EditorGUI.BeginChangeCheck();
          EditorGUI.showMixedValue = isAProp.hasMultipleDifferentValues;

          var field = (UnityEither) property.findFieldValueInObject(property.serializedObject.targetObject).get;
          popupOptions[0] = field.aDescription;
          popupOptions[1] = field.bDescription;

          var selectedIndex = isAProp.boolValue ? 0 : 1;
          selectedIndex = EditorGUI.Popup(popupRect, label.text, selectedIndex, popupOptions);
          var isChanged = EditorGUI.EndChangeCheck();
          if (isChanged) {
            if (selectedIndex == 0) {
              isAProp.boolValue = true;
              rightValueProp.setToDefaultValue();
            }
            else {
              isAProp.boolValue = false;
              leftValueProp.setToDefaultValue();
            }
          }

          if (!isAProp.hasMultipleDifferentValues) {
            drawProperty(selectedIndex == 0 ? leftValueProp : rightValueProp);
          }

          EditorGUI.EndProperty();
        }
        else {
          EditorGUI.LabelField(valueRect, "types not serializable!");
        }
        
      } else {
        Log.d.error("Can't find _isA serialized property");
      }
    }
  }
}