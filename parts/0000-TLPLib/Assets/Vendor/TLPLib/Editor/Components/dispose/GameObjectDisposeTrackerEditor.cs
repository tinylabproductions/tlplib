using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Components.dispose {
  [CustomEditor(typeof(GameObjectDisposeTracker))]
  public class GameObjectDisposeTrackerEditor : OdinEditor {
    public override void OnInspectorGUI() {
      var so = (GameObjectDisposeTracker) serializedObject.targetObject;
      EditorGUILayout.LabelField("Tracked objects:", so.trackedCount.ToString());
      EditorGUILayout.Space();

      foreach (var t in so.trackedDisposables) {
        EditorGUILayout.LabelField(t.asString());
      }
    }
  }
}