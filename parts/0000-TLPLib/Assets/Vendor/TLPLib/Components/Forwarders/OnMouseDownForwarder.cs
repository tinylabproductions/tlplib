using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace Assets.Vendor.TLPLib.Components.Forwarders {
  public class OnMouseDownForwarder : MonoBehaviour {
    private readonly Subject<Unit> _onMouseDown = new Subject<Unit>();
    public IObservable<Unit> onMouseDown { get { return _onMouseDown; } }

    // ReSharper disable once UnusedMember.Local
    private void OnMouseDown() { _onMouseDown.push(F.unit); }
  }
}
