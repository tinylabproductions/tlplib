using com.tinylabproductions.TLPLib.Logger;
using UnityEditor;
using UnityEngine;
using static com.tinylabproductions.TLPLib.unity_serialization.SerializedPropertyUtils;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  [CustomPropertyDrawer(typeof(HideByAttribute))]
  public class HideByPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var hideByAttribute = (HideByAttribute) attribute;
      var enabled = GetConditionalHideAttributeResult(hideByAttribute, property);

      var wasEnabled = GUI.enabled;
      GUI.enabled = enabled;
      if (!hideByAttribute.hideInInspector || enabled) {
        EditorGUI.PropertyField(position, property, label, true);
      }

      GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      var hideByAttribute = (HideByAttribute) attribute;
      var enabled = GetConditionalHideAttributeResult(hideByAttribute, property);

      if (!hideByAttribute.hideInInspector || enabled) {
        return EditorGUI.GetPropertyHeight(property, label);
      }
      else {
        return -EditorGUIUtility.standardVerticalSpacing;
      }
    }

    public bool GetConditionalHideAttributeResult(HideByAttribute condHAtt, SerializedProperty property) {
      var enabled = true;
      var propertyPath = property.propertyPath;
      var conditionPath = propertyPath.Replace(property.name, condHAtt.boolSourceField);
      
      if (getValueProp(property, conditionPath).valueOut(out var value)) {
        enabled = value.boolValue;
      }
      else {
        Log.d.warn(
          "Attempting to use a HideByAttribute but no matching SourcePropertyValue found in object: " +
          condHAtt.boolSourceField
        );
      }

      return enabled;
    }
  }
}
