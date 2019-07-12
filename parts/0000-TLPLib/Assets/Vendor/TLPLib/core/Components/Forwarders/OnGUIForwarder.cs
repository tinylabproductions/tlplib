using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnGUIForwarder : EventForwarder<Unit>, IMB_OnGUI {
    public void OnGUI() => _onEvent.push(F.unit);
  }
}