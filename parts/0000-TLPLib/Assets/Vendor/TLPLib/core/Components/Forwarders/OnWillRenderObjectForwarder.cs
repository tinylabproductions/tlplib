using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnWillRenderObjectForwarder : EventForwarder<Camera>, IMB_OnWillRenderObject {
    public void OnWillRenderObject() {
      if (Log.d.isVerbose()) Log.d.verbose(
        $"{nameof(OnWillRenderObjectForwarder)} this = {this}, camera current = {Camera.current}"
      );
      _onEvent.push(Camera.current);
    }
  }
}