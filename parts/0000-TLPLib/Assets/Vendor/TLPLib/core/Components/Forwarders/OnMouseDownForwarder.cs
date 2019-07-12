using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnMouseDownForwarder : EventForwarder<Unit>, IMB_OnMouseDown {
    public void OnMouseDown() => _onEvent.push(F.unit);
  }
}
