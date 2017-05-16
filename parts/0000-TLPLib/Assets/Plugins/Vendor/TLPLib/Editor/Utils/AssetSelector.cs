using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  class AssetSelector : EditorWindow, IMB_OnGUI {
    [UsedImplicitly, MenuItem("Tools/Select assets of type")]
    static void init() => ((AssetSelector) GetWindow(typeof(AssetSelector))).Show();

    // Useful to clean serialized assets after migration or unity version upgrade
    [UsedImplicitly, MenuItem("Tools/Make selected objects dirty")]
    static void makeObjectsDirty() {
      var objects = Selection.objects;
      Undo.RecordObjects(objects, "Set objects dirty");
      foreach (var o in objects) EditorUtility.SetDirty(o);
    }

    MonoScript script;

    public void OnGUI() {
      script = (MonoScript) EditorGUILayout.ObjectField("Type", script, typeof(MonoScript), false);
      if (script) {
        var type = script.GetClass();
        // sometimes this happens after code reload
        if (type == null) return;
        if (type.canBeUnityComponent()) {
          if (GUILayout.Button("Select all")) {
            try {
              EditorUtility.DisplayProgressBar("Working", "Please wait...", 0);
              var objects = AssetDatabase.FindAssets("t:prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath(path, type))
                .Where(c => c)
                .Select(c => {
                  foreach (var _ in F.opt(c as Component)) return _.gameObject;
                  foreach (var _ in F.opt(c as MonoBehaviour)) return _.gameObject;
                  throw new Exception($"Unrecognized type {c.GetType()} on component {c}");
                })
                .ToArray();
              Log.info($"Total objects found: {objects.Length}");
              foreach (var obj in objects) {
                Log.info(AssetDatabase.GetAssetPath(obj), obj);
              }
              Selection.objects = objects;
            }
            finally {
              EditorUtility.ClearProgressBar();
            }
          }
        }
        else {
          GUILayout.Label("Type should be MonoBehaviour, Component or interface");
        }
      }
    }
  }
}
