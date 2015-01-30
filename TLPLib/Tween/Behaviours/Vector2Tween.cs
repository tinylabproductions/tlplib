using System;
using UnityEngine;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  class Vector2Tween : BaseTween {
    public Vector2 from;
    public Vector2 to = Vector2.one;
    [Serializable] public class Vector2Event : UnityEvent<Vector2> { }
    public Vector2Event onChange;

    void act(float f) {
      onChange.Invoke(Vector2.Lerp(from, to, f));
    }

    public override GoTweenConfig config(GoTweenConfig cfg) {
      return cfg.floatCallbackProp(act);
    }
  }
}
