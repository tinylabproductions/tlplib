using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class CollisionEnter2DForwarder : MonoBehaviour {
    public delegate void OnEnter(Collision2D collision);

    public event OnEnter enter;

    internal void OnCollisionEnter2D(Collision2D collision) {
      if (enter == null) return;
      enter(collision);
    }
  }
}
