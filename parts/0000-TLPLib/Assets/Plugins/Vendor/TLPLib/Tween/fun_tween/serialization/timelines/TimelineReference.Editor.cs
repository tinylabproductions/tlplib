#if UNITY_EDITOR
using System;
using com.tinylabproductions.TLPLib.Functional;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  public partial class TimelineReference {
    public override Option<Act<float>> durationSetterOpt() => F.none_;
    public override Object[] getTargets() => new Object[]{};
  }
}
#endif