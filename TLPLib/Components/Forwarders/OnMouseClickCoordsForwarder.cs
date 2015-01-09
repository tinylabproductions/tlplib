using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnMouseClickCoordsForwarder : OnPointerClickBase {
    private readonly Subject<Vector3> _onMouseClick = new Subject<Vector3>();
    public IObservable<Vector3> onMouseClick { get { return _onMouseClick; } }

    public new OnMouseClickCoordsForwarder init(bool uguiBlocks) {
      base.init(uguiBlocks);
      return this;
    }

    protected override void pointerClick(Vector3 hitPoint) {
      _onMouseClick.push(hitPoint);
    }
  }
}
