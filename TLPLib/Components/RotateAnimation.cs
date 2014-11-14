using com.tinylabproductions.TLPLib.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class RotateAnimation : MonoBehaviour {
    public Vector3 rotation;

    [UsedImplicitly] void Update() {
      transform.Rotate(rotation * Time.deltaTime);
    }
  }
}
