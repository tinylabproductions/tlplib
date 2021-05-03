#if UNITY_EDITOR
using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using Sirenix.OdinInspector;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  public partial struct SerializedEaseV2 {
    [UsedImplicitly] bool displayPreview => SerializedTweenTimelineV2.editorDisplayEasePreview;

    [LazyProperty] static SerializedEaseSelector easeSelector => new();
    
    Texture2D _preview;

    partial void editor_invalidate() {
      _preview = null;
    }

    [OnInspectorGUI, PropertyOrder(-1)] void draw() {
      using (new EditorGUILayout.HorizontalScope()) {
        drawSelector();
        drawPreview();
      }
    }

    void drawPreview() {
      if (!_preview) {
        _preview = isSimple
          ? SerializedEasePreview.editorPreview(_simple)
          : (_complex != null ? SerializedEasePreview.generateTexture(ease) : null);
      }
      if (_preview) {
        // Yes, this draws a texture.
        GUILayout.Label(_preview, GUILayout.Width(80), GUILayout.Height(80));
      }
    }
    
    void drawSelector() {
      var currentSelection = _isComplex && _complex != null
        ? new SelectedEase(_complex.GetType())
        : new SelectedEase(_simple);

      var valueDisplayString = currentSelection.value.fold(_ => _.ToString(), _ => _.Name);
      
      var res = OdinSelector<SelectedEase>.DrawSelectorDropdown(GUIContent.none, valueDisplayString, rect => {
        var selector = easeSelector;
        selector.EnableSingleClickToSelect();
        selector.SetSelection(currentSelection);
        selector.ShowInPopup(rect, windowWidth: 200);
        return selector;
      });

      if (res != null && res.headOption().valueOut(out var newValue)) {
        if (newValue.value.valueOut(out var simple, out var type)) {
          if (newValue != currentSelection) {
            _isComplex = true;
            _complex = (IComplexSerializedEase) Activator.CreateInstance(type);
            invalidate();
          }
        }
        else {
          _isComplex = false;
          _simple = simple;
          _complex = null;
          invalidate();
        }
      }
    }
  }
  
  /// <summary>
  /// Helper class to show all available eases in a dropdown.
  /// </summary>
  class SerializedEaseSelector : OdinSelector<SelectedEase> {
    readonly (string path, SelectedEase ease, Texture2D texture)[] source;
    readonly OdinMenuStyle menuStyle;

    public SerializedEaseSelector() {
      menuStyle = OdinMenuStyle.TreeViewStyle.Clone();
      menuStyle.Height = 40;
      menuStyle.IconSize = 36;

      var simpleEases = ((SimpleSerializedEase[]) Enum.GetValues(typeof(SimpleSerializedEase)))
        .Select(val => {
          var path = val.ToString();
          path = toFolder(path, "InOut");
          path = toFolder(path, "In");
          path = toFolder(path, "Out");
          return (path, new SelectedEase(val), SerializedEasePreview.editorPreview(val));

          static string toFolder(string path, string folder) =>
            path.EndsWithFast(folder) ? $"{folder}/{path.ensureDoesNotEndWith(folder)}" : path;
        }).ToArray();

      var types = TypeCache.GetTypesDerivedFrom<SerializedEaseV2.IComplexSerializedEase>()
        .Select(type => {
          var path = $"Complex/{type.Name.ensureDoesNotStartWith("ComplexEase_")}";
          return (path, new SelectedEase(type), (Texture2D) null);
        })
        .ToArray();
      
      source = simpleEases.Concat(types).ToArray();
    }
    
    protected override void BuildSelectionTree(OdinMenuTree tree) {
      tree.Config.DrawSearchToolbar = true;
      tree.Selection.SupportsMultiSelect = false;
      tree.DefaultMenuStyle = menuStyle;

      foreach (var tpl in source) {
        tree.Add(tpl.path, tpl.ease, tpl.texture);
      }
    }
  }
}
#endif