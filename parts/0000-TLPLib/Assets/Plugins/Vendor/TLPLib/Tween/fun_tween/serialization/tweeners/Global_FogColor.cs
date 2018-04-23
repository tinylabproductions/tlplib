using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Global_FogColor : SerializedTweener<Color, Unit> {
    public Global_FogColor() : base(TweenOps.color, TweenMutators.globalFogColor, Defaults.color) { }
  }
}