using System;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  class FloatTween : BaseTween {
    public float from;
    public float to = 1;
    [Serializable] public class FloatEvent : UnityEvent<float> { }
    public FloatEvent onChange;

    public override GoTweenConfig config(GoTweenConfig cfg) {
      return cfg.floatCallbackProp(onChange.Invoke, from, to);
    }
  }
}
