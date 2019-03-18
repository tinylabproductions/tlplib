#if UNITY_EDITOR
using System;
using com.tinylabproductions.TLPLib.Functional;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  public partial class SerializedTweenTimelineElement {
    public abstract Option<Act<float>> durationSetterOpt();
    public abstract Object[] getTargets();
  }
}
#endif
