using com.tinylabproductions.TLPLib.validations;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  [CustomPropertyDrawer(typeof(UnityTagAttribute), useForChildren: true)]
  public class UnityTagDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
      property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
  }
}