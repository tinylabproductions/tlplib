using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Global_FogDensity : SerializedTweener<float, Unit> {
    public Global_FogDensity() : base(
      TweenOps.float_, SerializedTweenerOps.Add.float_, SerializedTweenerOps.Extract.globalFogDensity, 
      TweenMutators.globalFogDensity, Defaults.float_
    ) { }
  }
}