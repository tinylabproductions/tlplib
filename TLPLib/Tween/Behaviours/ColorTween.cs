using System;
using UnityEngine;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  class ColorTween : BaseTween {
    public Color from = Color.white;
    public Color to = Color.white;
    [Serializable] public class ColorEvent : UnityEvent<Color> { }
    public ColorEvent onChange;

    public override GoTweenConfig config(GoTweenConfig cfg) {
      return cfg.floatCallbackProp(f => onChange.Invoke(Color.Lerp(from, to, f)));
    }
  }
}
