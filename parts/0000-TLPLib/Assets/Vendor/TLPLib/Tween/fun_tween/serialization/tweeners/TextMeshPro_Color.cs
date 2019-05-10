using TMPro;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class TextMeshPro_Color : SerializedTweener<Color, TextMeshProUGUI> {
    public TextMeshPro_Color() : base(
      TweenOps.color, SerializedTweenerOps.Add.color, SerializedTweenerOps.Extract.tmProColor,
      TweenMutators.tmProColor, Defaults.color
    ) { }
  }
}