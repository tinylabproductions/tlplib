using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Tween.Behaviours;
using UnityEditor;
using UnityEngine;

namespace tlplib_editor.Tween {
  [CustomEditor(typeof(BaseTween), true)]
  class BaseTweenEditor : Editor {
    [UsedImplicitly]
    public override void OnInspectorGUI() {
      var binding = (BaseTween)target;
      DrawDefaultInspector();
      if (Application.isPlaying) {
        if (GUILayout.Button("Refresh")) {
          binding.editorRefresh();
        }
      }
    }
  }
}
