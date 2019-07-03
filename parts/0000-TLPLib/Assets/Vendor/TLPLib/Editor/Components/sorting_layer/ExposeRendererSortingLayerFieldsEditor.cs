using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer.Editor {
  [CustomEditor(typeof(ExposeRendererSortingLayerFields), true)]
  [CanEditMultipleObjects]
  public class ExposeRendererSortingLayerFieldsEditor : OdinEditor {
    Renderer[] renderers;

    public void OnEnable() {
      renderers =
        targets
        .collect(t => ((MonoBehaviour) t).GetComponent<Renderer>().opt())
        .ToArray();
    }

    public override void OnInspectorGUI() {
      SortingLayerHelper.drawSortingLayers(renderers);
    }
  }
}
