using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using pzd.lib.collection;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.log;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.debug {
  public class StateExposerEditorWindow : EditorWindow, IMB_OnGUI {
    [MenuItem("Tools/Window/State Exposer")]
    public static void OpenWindow() => GetWindow<StateExposerEditorWindow>("State Exposer").Show();
    
    static readonly LazyVal<GUIStyle> 
      multilineTextStyle = F.lazy(() => new GUIStyle {
        wordWrap = true, alignment = TextAnchor.UpperLeft, richText = false
      }),
      scopeKeyTextStyle = F.lazy(() => new GUIStyle { fontSize = 14, fontStyle = FontStyle.Bold }),
      objectInstanceTextStyle = F.lazy(() => new GUIStyle { fontStyle = FontStyle.Bold }),
      longLabelTextStyle = F.lazy(() => new GUIStyle { fontStyle = FontStyle.Bold });

    readonly HashSet<StructuralEquals<ImmutableList<StateExposer.ScopeKey>>> 
      expandObjects = new(), expandInnerScopes = new();

    float repaintEverySeconds = 0.1f;
    DateTime lastRepaint = DateTime.MinValue;
    Vector2 scrollViewPosition;

    [UsedImplicitly] void OnInspectorUpdate() {
      var now = DateTime.UtcNow;
      if (now >= lastRepaint.AddSeconds(repaintEverySeconds)) {
        Repaint();
        lastRepaint = now;
      }
    }
    
    public void OnGUI() {
      scrollViewPosition = EditorGUILayout.BeginScrollView(
        scrollViewPosition, alwaysShowHorizontal: false, alwaysShowVertical: false
      );
      try {
        using (new EditorGUILayout.HorizontalScope()) {
          repaintEverySeconds = EditorGUILayout.FloatField("Repaint every (seconds)", repaintEverySeconds);
          if (GUILayout.Button("Run GC")) {
            var previous = GC.GetTotalMemory(forceFullCollection: false);
            GC.Collect();
            var current = GC.GetTotalMemory(forceFullCollection: false);
            Log.d.info(
              $"Garbage collection performed, previous={previous.toBytesReadable()}, current={current.toBytesReadable()}"
            );
          }
        }

        renderScope(StateExposer.instance.rootScope, ImmutableList<StateExposer.ScopeKey>.Empty.structuralEquals());
      }
      finally {
        EditorGUILayout.EndScrollView();
      }

      void renderScope(StateExposer.Scope scope, StructuralEquals<ImmutableList<StateExposer.ScopeKey>> path) {
        var objects = scope.groupedData.ToArray();
        if (objects.nonEmpty() && foldout(expandObjects, $"Objects ({objects.Length})")) {
          using var _ = new EditorGUI.IndentLevelScope();
          renderObjects();
        }

        var scopes = scope.scopes;
        if (scopes.nonEmpty() && foldout(expandInnerScopes, $"Scopes ({scopes.Length})")) {
          using var _ = new EditorGUI.IndentLevelScope();
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
              using var _ = new EditorGUI.IndentLevelScope();
              if (isMultiline(data.value)) {
                renderLabel();
                using (new EditorGUI.IndentLevelScope()) render(data.value);
              }
              else using (new EditorGUILayout.HorizontalScope()) {
                renderLabel();
                EditorGUILayout.Separator();
                render(data.value);
              }

              void renderLabel() => EditorGUILayout.LabelField($"{data.name}:", longLabelTextStyle.strict);

              static bool isMultiline(StateExposer.IRenderableValue value) => value.match(
                stringValue: str => 
                  str.value.Contains('\n')
                  // Assume that if we have long lines they will wrap
                  || str.value.Length > 100,
                floatValue: _ => false,
                boolValue: _ => false,
                objectValue: _ => false,
                actionValue: _ => false,
                kVValue: kv => isMultiline(kv.key) || isMultiline(kv.value),
                enumerableValue: enumerable => enumerable.values.Count != 0,
                headerValue: _ => true
              );
              
              static void render(StateExposer.IRenderableValue value) => value.voidMatch(
                stringValue: str => EditorGUILayout.LabelField(str.value, multilineTextStyle.strict),
                floatValue: flt => EditorGUILayout.FloatField(flt.value, multilineTextStyle.strict),
                boolValue: b => EditorGUILayout.Toggle(b.value),
                objectValue: obj => EditorGUILayout.ObjectField(obj.value, typeof(Object), allowSceneObjects: true),
                actionValue: act => { if (GUILayout.Button(act.label)) act.value(); },
                kVValue: kv => {
                  using var _ = new EditorGUILayout.HorizontalScope();
                  render(kv.key);
                  render(kv.value);
                },
                enumerableValue: enumerable => {
                  using var _ = new EditorGUILayout.VerticalScope();
                  var first = true;
                  if (enumerable.showCount) {
                    EditorGUILayout.LabelField($"{enumerable.values.Count} elements", longLabelTextStyle.strict);
                  }
                  foreach (var value in enumerable.values) {
                    if (!first) EditorGUILayout.Separator();
                    render(value);
                    first = false;
                  }
                },
                headerValue: header => {
                  using var _ = new EditorGUILayout.VerticalScope();
                  render(header.header);
                  using (new EditorGUI.IndentLevelScope(header.indentBy)) render(header.value);
                }
              );
            }
          }
        }
        
        void renderScopes() {
          foreach (var (key, innerScope) in scopes.OrderBySafe(_ => _.Key.name)) {
            EditorGUILayout.LabelField(key.name, scopeKeyTextStyle.strict);
            {if (key.unityObject.valueOut(out var unityObject)) {
              using var _ = new EditorGUI.IndentLevelScope();
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