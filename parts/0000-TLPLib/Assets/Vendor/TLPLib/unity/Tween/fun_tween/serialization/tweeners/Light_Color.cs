using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Light_Color : SerializedTweener<Color, Light> {
    public Light_Color() : base(
      TweenOps.color, SerializedTweenerOps.Add.color, SerializedTweenerOps.Extract.lightColor,
      TweenMutatorsU.lightColor, Defaults.color
    ) { }
  }
}