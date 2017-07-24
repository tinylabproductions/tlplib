using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.GyroInput {
  public class GyroParallaxed : MonoBehaviour, IMB_Update {
#pragma warning disable 649
    [SerializeField] float zValue;
#pragma warning restore 649
    public bool enabled = true;
    public void Update() {
      if (enabled) {
        transform.localPosition = GyroOffset.instance.offset * -zValue;
      }
    }
  }
}