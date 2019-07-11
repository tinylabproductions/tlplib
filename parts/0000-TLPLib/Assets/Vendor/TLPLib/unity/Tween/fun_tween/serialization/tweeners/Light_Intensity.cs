using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Light_Intensity : SerializedTweener<float, Light> {
    public Light_Intensity() : base(
      TweenOps.float_, SerializedTweenerOps.Add.float_, SerializedTweenerOps.Extract.lightIntensity,
      TweenMutatorsU.lightIntensity, Defaults.float_
    ) { }
  }
}