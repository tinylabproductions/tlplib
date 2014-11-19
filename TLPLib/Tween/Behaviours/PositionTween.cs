
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  class PositionTween : BaseTween {
    public Vector3 postion;
    public bool isRelative;
    public bool isLocal = true;

    public override GoTweenConfig config(GoTweenConfig cfg) {
      return 
        isLocal ?
        cfg.localPosition(postion, isRelative) :
        cfg.position(postion, isRelative);
    }
  }
}
