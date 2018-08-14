 using com.tinylabproductions.TLPLib.Editor.extensions;
 using com.tinylabproductions.TLPLib.unity_serialization;
 using UnityEditor;
 using UnityEngine;

 namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
   [CustomPropertyDrawer(typeof(UnityOption), useForChildren: true), CanEditMultipleObjects]
   public class UnityOptionDrawer : PropertyDrawer {
     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
       EditorGUI.BeginProperty(position, label, property);

       Rect firstRect, secondRect;
       if (label.text == "" && label.image == null) {
         const float TOGGLE_WIDTH = 16;
         firstRect = new Rect(position.x, position.y, TOGGLE_WIDTH, position.height);
         secondRect = new Rect(position.x + TOGGLE_WIDTH, position.y, position.width - TOGGLE_WIDTH, position.height);
       }
       else {
         DrawerUtils.twoFieldsLabel(EditorGUI.IndentedRect(position), out firstRect, out secondRect);
       }

       var isSomeProp = property.FindPropertyRelative("_isSome");
       var valueProp = property.FindPropertyRelative("_value");
       EditorGUI.BeginChangeCheck();
       EditorGUI.showMixedValue = isSomeProp.hasMultipleDifferentValues;
       var isSome = EditorGUI.ToggleLeft(firstRect, label, isSomeProp.boolValue);
       var someChanged = EditorGUI.EndChangeCheck();
       if (someChanged) isSomeProp.boolValue = isSome;
       if (isSome) {
         EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
         EditorGUI.PropertyField(secondRect, valueProp, GUIContent.none);
       }
       else {
         if (someChanged) valueProp.setToDefaultValue();
       }

       EditorGUI.EndProperty();
     }
   }
 }