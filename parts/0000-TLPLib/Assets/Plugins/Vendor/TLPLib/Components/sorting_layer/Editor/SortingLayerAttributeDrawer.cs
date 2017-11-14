using com.tinylabproductions.TLPLib.Extensions;
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

      var selected = -1;

      var layers = SortingLayer.layers;
      var names = layers.map(_ => _.name);
      if (!property.hasMultipleDifferentValues) {
        for (var i = 0; i < layers.Length; i++) {
          if (property.intValue == layers[i].id) selected = i;
        }
      }

      EditorGUI.BeginProperty(position, label, property);
      position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
      selected = EditorGUI.Popup(position, selected, names);
      EditorGUI.EndProperty();

      if (selected >= 0) property.intValue = layers[selected].id;
    }
  }
}