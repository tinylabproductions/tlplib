using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class RectTransform_AnchoredPosition : SerializedTweener<Vector2, RectTransform> {
    public RectTransform_AnchoredPosition() : base(
      TweenOps.vector2, SerializedTweenerOps.Add.vector2, SerializedTweenerOps.Extract.anchoredPosition, 
      TweenMutators.anchoredPosition, Defaults.vector2
    ) {}
  }
}