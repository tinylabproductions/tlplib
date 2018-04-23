using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Graphic_ColorAlpha : SerializedTweener<float, Graphic> {
    public Graphic_ColorAlpha() : base(TweenOps.float_, TweenMutators.graphicColorAlpha, Defaults.alpha) { }
  }
}