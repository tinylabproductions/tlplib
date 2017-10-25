using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnDestroyForwarder : EventForwarder<Unit>, IMB_OnDestroy {
    public void OnDestroy() => _onEvent.push(F.unit);
  }
}