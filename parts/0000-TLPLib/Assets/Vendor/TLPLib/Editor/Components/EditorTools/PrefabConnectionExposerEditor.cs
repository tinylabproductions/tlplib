using System;
using com.tinylabproductions.TLPLib.Logger;
using Sirenix.OdinInspector.Editor;
using Smooth.Collections;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  [CustomEditor(typeof(PrefabConnectionExposer))]
  public class PrefabConnectionExposerEditor : OdinEditor {
    public override void OnInspectorGUI() {
      var so = (PrefabConnectionExposer) serializedObject.targetObject;

      var instanceTransform = so.instance.transform;
      var prefabTransform = so.prefab.transform;

      void run<A>(
        string name, Fn<Transform, A> get, Act<Transform, A> set
      ) {
        var instanceV = get(instanceTransform);
        var prefabV = get(prefabTransform);
        var same = EqComparer<A>.Default.Equals(instanceV, prefabV);
        EditorGUILayout.LabelField($"{name} in prefab", same ? "same" : prefabV.ToString());
        if (!same) {
          if (GUILayout.Button($"Apply current {name.ToLower()} to prefab")) {
            set(prefabTransform, instanceV);
            if (Log.d.isDebug())
              Log.d.debug($"{name} set to {instanceV} (was {prefabV}) on prefab.", so.prefab);
          }
        }
      }

      if (GUILayout.Button("Select prefab")) {
        Selection.activeObject = so.prefab;
      }
      GUILayoutUtility.GetRect(6f, 24f);
      
      run("Position", _ => _.localPosition, (t, v) => t.localPosition = v);
      GUILayoutUtility.GetRect(6f, 24f);
      run("Rotation", _ => _.localRotation, (t, v) => t.localRotation = v);
      GUILayoutUtility.GetRect(6f, 24f);
      run("Scale", _ => _.localScale, (t, v) => t.localScale = v);
    }
  }
}