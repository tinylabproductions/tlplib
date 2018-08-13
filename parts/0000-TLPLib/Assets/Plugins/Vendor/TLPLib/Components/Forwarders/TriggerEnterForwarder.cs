using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class TriggerEnterForwarder : EventForwarder<Collider>, IMB_OnTriggerEnter {
    public void OnTriggerEnter(Collider other) => _onEvent.push(other);
  }
}