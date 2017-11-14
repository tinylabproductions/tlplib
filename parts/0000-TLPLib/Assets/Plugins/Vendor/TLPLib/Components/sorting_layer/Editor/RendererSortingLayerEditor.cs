using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer.Editor {
  [CustomEditor(typeof(RendererSortingLayer), true)]
  [CanEditMultipleObjects]
  public class RendererSortingLayerEditor : UnityEditor.Editor {
    Renderer[] renderers;

    public void OnEnable() {
      renderers = targets.Select(t => ((MonoBehaviour)t).GetComponent<Renderer>()).Where(r => r != null).ToArray();
    }

    public override void OnInspectorGUI() {
      SortingLayerHelper.drawSortingLayers(renderers);
    }
  }
}
