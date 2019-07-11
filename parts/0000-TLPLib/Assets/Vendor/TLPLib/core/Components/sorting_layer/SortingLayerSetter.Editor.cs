#if UNITY_EDITOR
using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer {
  [ExecuteInEditMode]
  public partial class SortingLayerSetter : IMB_Update {
    public void Update() {
      if (Application.isPlaying || !sortingLayer) return;

      var extracted = extract();
      var sortingLayerMatches =
        extracted.layerId == sortingLayer.sortingLayer
        && extracted.order == sortingLayer.orderInLayer;

      if (!sortingLayerMatches) {
        recordEditorChanges();
        apply(sortingLayer);
      }
    }
  }
}
#endif
