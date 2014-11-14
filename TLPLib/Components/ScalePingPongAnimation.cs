using com.tinylabproductions.TLPLib.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class ScalePingPongAnimation : MonoBehaviour {
    public Vector3 startScale, endScale;
    public float durationS;

    [UsedImplicitly]
    void Update() {
      var d = Mathf.PingPong(Time.time, durationS) / durationS;
      transform.localScale = startScale + (endScale - startScale) * d;
    }
  }
}
