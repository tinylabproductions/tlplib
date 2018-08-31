using com.tinylabproductions.TLPLib.Logger;
using UnityEditor;
using UnityEngine;

namespace Plugins.Vendor.TLPLib.Editor.CustomEditors {
  public class GenericUnityEitherParent {}

  [CustomPropertyDrawer(typeof(GenericUnityEitherParent))]
  public class UnityEitherDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      
      EditorGUI.BeginProperty(position, label, property);
      
      var indent = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;
      
      Log.d.warn("aliooo");
      
      SerializedProperty a = property.FindPropertyRelative("a");
      SerializedProperty b = property.FindPropertyRelative("b");
      
      position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
      var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);
      
      
      EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("a"), GUIContent.none);
      
      EditorGUI.indentLevel = indent;

      EditorGUI.EndProperty();
      

    }
    
  }
}