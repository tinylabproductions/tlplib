using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnDestroyForwarder : MonoBehaviour, IMB_OnDestroy {
    readonly Promise<Unit> _onDestroy;
    [PublicAPI] public readonly Future<Unit> onEvent;

    OnDestroyForwarder() {
      onEvent = Future<Unit>.async(out _onDestroy);
    }

    public void OnDestroy() => _onDestroy.complete(F.unit);
  }
}