using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Image_FillAmount : SerializedTweener<float, Image> {
    public Image_FillAmount() : base(
      TweenOps.float_, SerializedTweenerOps.Add.float_, SerializedTweenerOps.Extract.imageFillAmount,
      TweenMutators.imageFillAmount, Defaults.float_
    ) { }
  }
}