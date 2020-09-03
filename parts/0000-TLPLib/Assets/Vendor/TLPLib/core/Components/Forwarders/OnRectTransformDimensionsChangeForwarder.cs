using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.reactive;

using pzd.lib.functional;
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