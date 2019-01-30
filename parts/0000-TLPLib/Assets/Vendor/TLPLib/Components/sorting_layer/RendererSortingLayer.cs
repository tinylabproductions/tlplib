using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer {
  [RequireComponent(typeof(Renderer))]
  public sealed class RendererSortingLayer : SortingLayerSetter {
    new Renderer renderer => GetComponent<Renderer>();

    protected override void recordEditorChanges() =>
      renderer.recordEditorChanges("Renderer sorting layer changed");

    protected override void apply(SortingLayerReference sortingLayer) =>
      sortingLayer.applyTo(renderer);

    protected override SortingLayerAndOrder extract() {
      var renderer = this.renderer;
      return new SortingLayerAndOrder(renderer.sortingLayerID, renderer.sortingOrder);
    }
  }
}
