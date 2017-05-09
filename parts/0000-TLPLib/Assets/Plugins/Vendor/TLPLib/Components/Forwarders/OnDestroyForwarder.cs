using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnDestroyForwarder : MonoBehaviour, IMB_OnDestroy {
    readonly Subject<Unit> _onDestroy = new Subject<Unit>();
    public IObservable<Unit> onDestroy => _onDestroy;

    public void OnDestroy() => _onDestroy.push(F.unit);
  }
}