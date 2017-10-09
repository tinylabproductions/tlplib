﻿using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnRenderObjectForwarder : EventForwarder<Camera>, IMB_OnRenderObject {
    public void OnRenderObject() {
      if (Log.isVerbose) Log.verbose(
        $"{nameof(OnRenderObjectForwarder)} this = {this}, camera current = {Camera.current}"
      );
      _onEvent.push(Camera.current);
    }
  }
}