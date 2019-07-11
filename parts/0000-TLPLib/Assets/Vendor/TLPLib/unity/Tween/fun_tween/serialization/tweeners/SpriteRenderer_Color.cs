using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class SpriteRenderer_Color : SerializedTweener<Color, SpriteRenderer> {
    public SpriteRenderer_Color() : base(
      TweenOps.color, SerializedTweenerOps.Add.color, SerializedTweenerOps.Extract.spriteRendererColor,
      TweenMutatorsU.spriteRendererColor, Defaults.color
    ) { }
  }
}