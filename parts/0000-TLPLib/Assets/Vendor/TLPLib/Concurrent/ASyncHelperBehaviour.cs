using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  class ASyncHelperBehaviour : MonoBehaviour, 
    IMB_OnApplicationPause, IMB_OnApplicationQuit, IMB_LateUpdate
  {
    readonly Subject<bool> _onPause = new Subject<bool>();
    public IObservable<bool> onPause => _onPause;
    public void OnApplicationPause(bool paused) => _onPause.push(paused);

    readonly Subject<Unit> _onQuit = new Subject<Unit>();
    public IObservable<Unit> onQuit => _onQuit;
    public void OnApplicationQuit() => _onQuit.push(F.unit);

    readonly Subject<Unit> _onLateUpdate = new Subject<Unit>();
    public IObservable<Unit> onLateUpdate => _onLateUpdate;
    public void LateUpdate() => _onLateUpdate.push(F.unit);
  }
}
