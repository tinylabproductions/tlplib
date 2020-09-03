using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.reactive;

using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class ASyncHelperBehaviour : MonoBehaviour,
    IMB_OnApplicationPause, IMB_OnApplicationQuit, IMB_LateUpdate
  {
    readonly Subject<bool> _onPause = new Subject<bool>();
    public IRxObservable<bool> onPause => _onPause;
    public void OnApplicationPause(bool paused) => _onPause.push(paused);

    readonly Subject<Unit> _onQuit = new Subject<Unit>();
    public IRxObservable<Unit> onQuit => _onQuit;
    public void OnApplicationQuit() => _onQuit.push(F.unit);

    readonly Subject<Unit> _onLateUpdate = new Subject<Unit>();
    public IRxObservable<Unit> onLateUpdate => _onLateUpdate;
    public void LateUpdate() => _onLateUpdate.push(F.unit);
  }

  // Don't implement unity interfaces if not used
  class ASyncHelperBehaviourEmpty : MonoBehaviour {}
}
