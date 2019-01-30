using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnRectTransformDimensionsChangeForwarder : UIBehaviour {
    readonly Subject<Unit> _rectDimensionsChanged = new Subject<Unit>();

    public IRxObservable<Unit> rectDimensionsChanged => _rectDimensionsChanged;
    public RectTransform rectTransform => (RectTransform) transform;

    protected override void OnRectTransformDimensionsChange() => _rectDimensionsChanged.push(F.unit);
  }
}