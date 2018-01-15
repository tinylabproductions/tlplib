using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer {
  [RequireComponent(typeof(Canvas))]
  public sealed class CanvasSortingLayer : SortingLayerSetter {
    Canvas canvas => GetComponent<Canvas>();

    protected override void recordEditorChanges() => 
      canvas.recordEditorChanges("Canvas sorting layer changed");

    protected override void apply(SortingLayerReference sortingLayer) => 
      sortingLayer.applyTo(canvas);

    protected override SortingLayerAndOrder extract() {
      var canvas = this.canvas;
      return new SortingLayerAndOrder(canvas.sortingLayerID, canvas.sortingOrder);
    }
  }
}
