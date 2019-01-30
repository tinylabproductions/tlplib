 using com.tinylabproductions.TLPLib.Data;
 using UnityEditor;
 using UnityEngine;

 namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
   [CustomPropertyDrawer(typeof(Duration))]
   public class DurationDrawer : PropertyDrawer {
     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
       EditorGUI.BeginProperty(position, label, property);
       
       // ReSharper disable once LocalNameCapturedOnly
       var millisP = property.FindPropertyRelative("_millis");
       var seconds = millisP.intValue / 1000f;
      
       EditorGUI.BeginChangeCheck();
       DrawerUtils.twoFields(position, label, out var secondsPosition, out var millisPosition);
       using (new EditorIndent(0)) {
         var newSeconds = EditorGUI.FloatField(secondsPosition, seconds);
         EditorGUI.LabelField(secondsPosition, "s", DrawerUtils.overlayStyle());
         var newMillis = EditorGUI.IntField(millisPosition, Mathf.RoundToInt(newSeconds * 1000));
         EditorGUI.LabelField(millisPosition, "ms", DrawerUtils.overlayStyle());
         if (EditorGUI.EndChangeCheck()) {
           millisP.intValue = newMillis;
         }
       }
       
       EditorGUI.EndProperty();
     }
   }
 }