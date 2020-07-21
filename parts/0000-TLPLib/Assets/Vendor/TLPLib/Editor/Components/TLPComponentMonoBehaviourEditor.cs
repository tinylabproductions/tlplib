using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Components {

  [CustomEditor(typeof(TLPComponentMonoBehaviour), editorForChildClasses: true)]
  public class TLPComponentMonoBehaviourEditor : OdinEditor {
    public override void OnInspectorGUI() {
      if (GeneralDrawerConfig.Instance.ShowMonoScriptInEditor) {
        // standalone editor
        GUIHelper.PushGUIEnabled(false);
        base.OnInspectorGUI();
        GUIHelper.PopGUIEnabled();
      }
      else {
        // inline editor
        base.OnInspectorGUI();
      }
    }
  }
}