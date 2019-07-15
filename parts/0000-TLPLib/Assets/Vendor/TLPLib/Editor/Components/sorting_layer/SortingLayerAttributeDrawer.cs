using com.tinylabproductions.TLPLib.Editor.gui;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.functional;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer.Editor {
  [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
  public class SortyingLayerPropertyDrawer : PropertyDrawer {
    //https://forum.unity.com/threads/sorting-layer-vs-layer-mask-scripting.339444/#post-2451899
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      if (property.propertyType != SerializedPropertyType.Integer) {
        EditorGUI.LabelField(position, label.text, $"Use {nameof(SortingLayerAttribute)} only with int fields");
        return;
      }

      var selectedIdx = Option<int>.None;

      var layers = SortingLayer.layers;
      var names = layers.map(_ => _.name);
      if (!property.hasMultipleDifferentValues) {
        selectedIdx = layers.indexWhere(_ => _.id == property.intValue);
      }

      EditorGUI.BeginProperty(position, label, property);
      position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
      selectedIdx = EditorGUI_.IndexPopup(position, selectedIdx, names);
      EditorGUI.EndProperty();

      foreach (var idx in selectedIdx)
        property.intValue = layers[idx].id;
    }
  }
}