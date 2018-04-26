using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Light_Color : SerializedTweener<Color, Light> {
    public Light_Color() : base(TweenOps.color, TweenMutators.lightColor, Defaults.color) { }
  }
}