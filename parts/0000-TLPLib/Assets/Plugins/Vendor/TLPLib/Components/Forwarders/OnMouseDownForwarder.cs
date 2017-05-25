using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace Assets.Vendor.TLPLib.Components.Forwarders {
  public class OnMouseDownForwarder : MonoBehaviour, IMB_OnMouseDown {
    readonly Subject<Unit> _onMouseDown = new Subject<Unit>();
    public IObservable<Unit> onMouseDown => _onMouseDown;

    // ReSharper disable once UnusedMember.Local
    public void OnMouseDown() => _onMouseDown.push(F.unit);
  }
}
