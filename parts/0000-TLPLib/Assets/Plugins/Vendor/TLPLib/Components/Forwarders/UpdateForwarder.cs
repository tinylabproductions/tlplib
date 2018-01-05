using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Components {
  public class UpdateForwarder : EventForwarder<Unit>, IMB_Update {
    public void Update() => _onEvent.push(F.unit);
  }
}
