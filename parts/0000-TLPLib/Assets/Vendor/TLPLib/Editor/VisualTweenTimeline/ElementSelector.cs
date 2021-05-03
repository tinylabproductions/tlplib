using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using pzd.lib.exts;
using pzd.lib.functional;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  [Record] public partial class ElementSelectorResult {
    public readonly Type type;
    public readonly FieldInfo typeField;
    public readonly Object candidate;

    public ISerializedTweenTimelineElementBase createElement() {
      var newElement = (ISerializedTweenTimelineElementBase) Activator.CreateInstance(type);
      typeField.SetValue(newElement, candidate);
      return newElement;
    }
  }

  /// <summary>
  /// Show selector popup for timeline elements filtered for specific object.
  /// Used in Drag & Drop action in Timeline editor.
  /// </summary>
  public class ElementSelector : OdinSelector<ElementSelectorResult> {
    readonly ElementSelectorResult[] source;

    public ElementSelector(Object targetObject) {
      var types = TypeCache.GetTypesDerivedFrom<ISerializedTweenTimelineElementBase>()
        .Where(_ => !_.IsAbstract)
        .ToArray();
      
      var withElement = types.collect(type => 
        type.getAllFields()
        .Where(field => field.isSerializable() && typeof(Object).IsAssignableFrom(field.FieldType))
        .headOption()
        .map(_ => (type, field: _))
      ).ToArray();

      var possibleTargetObjects = new List<Object> { targetObject };
      {if (targetObject is GameObject go) {
        possibleTargetObjects.AddRange(go.GetComponents<Component>());
      }}
      
      source = withElement.collect(tpl => {
        foreach (var targetObject in possibleTargetObjects) {
          if (tpl.field.FieldType.IsInstanceOfType(targetObject)) {
            return Some.a(new ElementSelectorResult(tpl.type, tpl.field, targetObject));
          }
        }
        return None._;
      }).ToArray();
    }
    
    protected override void BuildSelectionTree(OdinMenuTree tree) {
      tree.Config.DrawSearchToolbar = true;
      tree.Selection.SupportsMultiSelect = false;

      foreach (var res in source) {
        tree.Add(res.type.Name, res);
      }
    }
  }
}