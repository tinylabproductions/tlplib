using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  [CustomEditor(typeof(FunTweenManagerV2))]
  public class FunTweenManagerV2Editor : OdinEditor {
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      if (GUILayout.Button("Open Timeline Editor")) {
        TimelineEditor.showWindow();
      }
    }
  }
}
