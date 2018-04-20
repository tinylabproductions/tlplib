using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Graphic_Color : SerializedTweener<Color, Graphic> {
    public Graphic_Color() : base(TweenOps.color, TweenMutators.graphicColor, Defaults.color) { }
  }
}