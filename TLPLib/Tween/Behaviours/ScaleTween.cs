
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  class ScaleTween : BaseTween {
    public Vector3 scale;
    public bool isRelative;

    public override GoTweenConfig config(GoTweenConfig cfg) {
      return cfg.scale(scale, isRelative);
    }
  }
}
