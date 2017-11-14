using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer {
  [CreateAssetMenu]
  public class SortingLayerReference : ScriptableObject {
    [SerializeField, SortingLayer] int _sortingLayer;
    [SerializeField] int _orderInLayer;

    public int sortingLayer => _sortingLayer;
    public int orderInLayer => _orderInLayer;

    public void applyTo(Canvas canvas) {
      canvas.sortingLayerID = sortingLayer;
      canvas.sortingOrder = orderInLayer;
    }
  }
}
