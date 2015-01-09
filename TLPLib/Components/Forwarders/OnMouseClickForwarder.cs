using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnMouseClickForwarder : OnPointerClickBase {
    private readonly Subject<Unit> _onMouseClick = new Subject<Unit>();
    public IObservable<Unit> onMouseClick { get { return _onMouseClick; } }

    public new OnMouseClickForwarder init(bool ignoreIfUGUIClicked) {
      base.init(ignoreIfUGUIClicked);
      return this;
    }

    protected override void pointerClick(Vector3 hitPoint) { _onMouseClick.push(F.unit); }
  }
}
