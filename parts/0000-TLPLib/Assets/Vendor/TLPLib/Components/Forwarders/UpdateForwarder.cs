using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Components {
  /**
   * UpdateForwarder addresses an issue where ASync.EveryFrame creates a new
   * coroutine and drops it if GameObject is disabled.
   * UpdateForwarder does not drop subscriptions if GameObject is disabled.
   */
  public class UpdateForwarder : EventForwarder<Unit>, IMB_Update {
    public void Update() => _onEvent.push(F.unit);
  }
}
