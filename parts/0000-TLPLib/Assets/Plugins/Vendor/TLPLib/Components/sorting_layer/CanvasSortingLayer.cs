using com.tinylabproductions.TLPLib.Components.Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer {
  [RequireComponent(typeof(Canvas))]
  public partial class CanvasSortingLayer : MonoBehaviour, IMB_Awake {
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized
    [SerializeField, NotNull] SortingLayerReference sortingLayer;
    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649

    public void Awake() {
      if (!Application.isPlaying) return;
      var canvas = GetComponent<Canvas>();
      sortingLayer.applyTo(canvas);
    }
  }
}
