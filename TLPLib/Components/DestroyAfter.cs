using com.tinylabproductions.TLPLib.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  class DestroyAfter : MonoBehaviour {
    public float delay = 1f;

    [UsedImplicitly] void Start() {
      Destroy(gameObject, delay);
    }
  }
}
