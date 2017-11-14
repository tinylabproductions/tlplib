#if UNITY_EDITOR
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer {
  [ExecuteInEditMode]
  public partial class CanvasSortingLayer: IMB_Update {
    public void Update() {
      if (Application.isPlaying) return;
      var canvas = GetComponent<Canvas>();
      if (!canvas || !sortingLayer) return;
      var sortingLayerMatches =
        canvas.sortingLayerID == sortingLayer.sortingLayer &&
        canvas.sortingOrder == sortingLayer.orderInLayer;
      if (!sortingLayerMatches) {
        canvas.recordEditorChanges("Canvas layer changed");
        sortingLayer.applyTo(canvas);
      }
    }
  }
}
#endif
