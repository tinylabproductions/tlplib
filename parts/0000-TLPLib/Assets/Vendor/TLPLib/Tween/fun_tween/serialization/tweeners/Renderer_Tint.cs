using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Renderer_Tint : SerializedTweener<Color, Renderer> {
    public Renderer_Tint() : base(
      TweenOps.color, SerializedTweenerOps.Add.color, SerializedTweenerOps.Extract.rendererTint, 
      TweenMutators.rendererTint, Defaults.color
    ) { }
  }
}