using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.debug {
  [CustomEditor(typeof(InspectorStateExposer))]
  public class InspectorStateExposerEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      var so = (InspectorStateExposer) serializedObject.targetObject;
      var first = true;
      foreach (var group in so.groupedData) {
        if (!first) GUILayoutUtility.GetRect(24f, 24f);
        EditorGUILayout.LabelField($"For {group.Key} ({group.Key.GetHashCode()}):");
        EditorGUILayout.Space();
        foreach (var data in group) {
          data.value.voidMatch(
            stringValue: str => EditorGUILayout.LabelField(data.name, str.value),
            floatValue: flt => EditorGUILayout.FloatField(data.name, flt.value),
            objectValue: obj => EditorGUILayout.ObjectField(
              data.name, obj.value, typeof(Object), allowSceneObjects: true
            ),
            actionValue: act => { if (GUILayout.Button(data.name)) act.value(); }
          );
        }

        first = false;
      }

      // Update inspector every frame.
      Repaint();
    }
  }
}