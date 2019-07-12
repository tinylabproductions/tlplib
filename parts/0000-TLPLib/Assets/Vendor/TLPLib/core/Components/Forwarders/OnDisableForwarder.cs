using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnDisableForwarder : EventForwarder<Unit>, IMB_OnDisable {
    public void OnDisable() => _onEvent.push(F.unit);
  }
}