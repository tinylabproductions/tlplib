using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class SpriteRenderer_Color : SerializedTweener<Color, SpriteRenderer> {
    public SpriteRenderer_Color() : base(TweenOps.color, TweenMutators.spriteRendererColor, Defaults.color) { }
  }
}