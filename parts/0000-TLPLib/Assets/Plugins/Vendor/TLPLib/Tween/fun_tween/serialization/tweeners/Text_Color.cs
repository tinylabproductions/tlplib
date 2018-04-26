using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Text_Color : SerializedTweener<Color, Text> {
    public Text_Color() : base(TweenOps.color, TweenMutators.textColor, Defaults.color) { }
  }
}