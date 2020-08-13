using com.tinylabproductions.TLPLib.Components.Interfaces;
using pzd.lib.concurrent;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnDestroyForwarder : MonoBehaviour, IMB_OnDestroy {
    readonly Promise<Unit> _onDestroy;
    [PublicAPI] public readonly Future<Unit> onEvent;

    OnDestroyForwarder() {
      onEvent = Future.async<Unit>(out _onDestroy);
    }

    public void OnDestroy() => _onDestroy.complete(F.unit);
  }
}