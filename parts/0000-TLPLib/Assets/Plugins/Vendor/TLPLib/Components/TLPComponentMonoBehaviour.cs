using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public abstract class TLPComponentMonoBehaviour : MonoBehaviour {
    protected virtual void Reset() {
      // we may want to set hide flags here
    }
  }
}