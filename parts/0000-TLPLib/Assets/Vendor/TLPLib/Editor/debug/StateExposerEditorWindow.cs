using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Editor.gui;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.collection;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.debug {
  public class StateExposerEditorWindow : EditorWindow, IMB_OnGUI {
    [MenuItem("Tools/Window/State Exposer")]
    public static void OpenWindow() => GetWindow<StateExposerEditorWindow>("State Exposer").Show();
    
    static readonly LazyVal<GUIStyle> 
      multilineTextStyle = F.lazy(() => new GUIStyle {wordWrap = true}),
      scopeKeyTextStyle = F.lazy(() => new GUIStyle { fontSize = 14, fontStyle = FontStyle.Bold }),
      objectInstanceTextStyle = F.lazy(() => new GUIStyle { fontStyle = FontStyle.Bold }),
      longLabelTextStyle = F.lazy(() => new GUIStyle { fontStyle = FontStyle.Bold });

    readonly HashSet<StructuralEquals<ImmutableList<StateExposer.ScopeKey>>> 
      expandObjects = new(), expandInnerScopes = new();

    Vector2 scrollViewPosition;

    public void OnGUI() {
      scrollViewPosition = EditorGUILayout.BeginScrollView(
        scrollViewPosition, alwaysShowHorizontal: false, alwaysShowVertical: false
      );
      try {
        renderScope(StateExposer.instance.rootScope, ImmutableList<StateExposer.ScopeKey>.Empty.structuralEquals());
      }
      finally {
        EditorGUILayout.EndScrollView();
      }

      void renderScope(StateExposer.Scope scope, StructuralEquals<ImmutableList<StateExposer.ScopeKey>> path) {
        var objects = scope.groupedData.ToArray();
        if (objects.nonEmpty() && foldout(expandObjects, $"Objects ({objects.Length})")) {
          using var _ = EditorGUI_.indented();
          renderObjects();
        }

        var scopes = scope.scopes;
        if (scopes.nonEmpty() && foldout(expandInnerScopes, $"Scopes ({scopes.Length})")) {
          using var _ = EditorGUI_.indented();
          renderScopes();
        }

        void renderObjects() {
          foreach (var grouping in objects) {
            var maybeInstance = grouping.Key;
            
            EditorGUILayout.LabelField(
              maybeInstance.fold("Static", obj => $"instance: {obj} ({obj.GetHashCode()})"),
              objectInstanceTextStyle.strict
            );
            foreach (var data in grouping) {
              // If the label is longer than that then it gets truncated.
              const int MAX_LABEL_LENGTH = 12;
              using var _ = EditorGUI_.indented();
              data.value.voidMatch(
                stringValue: str => {
                  if (data.name.Length <= MAX_LABEL_LENGTH)
                    EditorGUILayout.LabelField(data.name, str.value, multilineTextStyle.strict);
                  else {
                    EditorGUILayout.LabelField($"{data.name}:", longLabelTextStyle.strict);
                    using (EditorGUI_.indented()) EditorGUILayout.LabelField(str.value, multilineTextStyle.strict);
                  }
                },
                floatValue: flt => EditorGUILayout.FloatField(data.name, flt.value, multilineTextStyle.strict),
                boolValue: b => EditorGUILayout.Toggle(data.name, b.value),
                objectValue: obj => EditorGUILayout.ObjectField(
                  data.name, obj.value, typeof(Object), allowSceneObjects: true
                ),
                actionValue: act => { if (GUILayout.Button(data.name)) act.value(); }
              );
            }
          }
        }
        
        void renderScopes() {
          foreach (var (key, innerScope) in scopes) {
            EditorGUILayout.LabelField(key.name, scopeKeyTextStyle.strict);
            {if (key.unityObject.valueOut(out var unityObject)) {
              using var _ = EditorGUI_.indented();
              EditorGUILayout.ObjectField(unityObject, typeof(Object), allowSceneObjects: true);
            }}

            renderScope(innerScope, path.collection.Add(key).structuralEquals());
          }
        }

        bool foldout(ISet<StructuralEquals<ImmutableList<StateExposer.ScopeKey>>> set, string name) {
          var ret = EditorGUILayout.Foldout(set.Contains(path), name);
          if (ret) set.Add(path);
          else set.Remove(path);
          return ret;
        }
      }
    }
  }
}