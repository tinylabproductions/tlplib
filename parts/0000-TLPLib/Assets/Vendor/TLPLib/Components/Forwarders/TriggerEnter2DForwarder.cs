using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class TriggerEnter2DForwarder : MonoBehaviour {
    public delegate void OnEnter(Collider2D other);

    public event OnEnter enter;

    internal void OnTriggerEnter2D(Collider2D other) {
      if (enter == null) return;
      enter(other);
    }
  }
}
