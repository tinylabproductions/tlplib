
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  class RotationTween : BaseTween {
    public Vector3 rotation;
    public bool isRelative;
    public bool isLocal = true;

    public override GoTweenConfig config(GoTweenConfig cfg) {
      return 
        isLocal ? 
        cfg.localRotation(rotation, isRelative) : 
        cfg.rotation(rotation, isRelative);
    }
  }
}
