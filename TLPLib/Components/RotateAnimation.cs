using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class RotateAnimation : MonoBehaviour {
    public Vector3 rotation;
    Quaternion initial;

    internal void Start() {
      initial = transform.localRotation;
    }

    internal void Update() {
      transform.Rotate(rotation * Time.deltaTime);
    }

    public void reset() {
      transform.localRotation = initial;
    }
  }
}
