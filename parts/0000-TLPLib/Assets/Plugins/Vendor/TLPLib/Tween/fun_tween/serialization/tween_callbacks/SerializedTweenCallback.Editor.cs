#if UNITY_EDITOR
using System;
using com.tinylabproductions.TLPLib.Functional;
namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks {
  public partial class SerializedTweenCallback {
    public override Option<Act<float>> durationSetterOpt() => F.none_;
    public override UnityEngine.Object[] getTargets() => new UnityEngine.Object[]{};
  }
}
#endif