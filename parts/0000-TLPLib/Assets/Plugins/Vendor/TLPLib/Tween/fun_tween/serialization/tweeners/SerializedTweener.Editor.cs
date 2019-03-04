#if UNITY_EDITOR
using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  public abstract partial class SerializedTweener<SourceType, DestinationType, Target> {
    public override Option<Act<float>> durationSetterOpt() => 
      F.some<Act<float>>(dur => _duration = dur);

    public override Object[] getTargets() => _targets as Object[];

    // Remove this after tlplib is no longer dependant on Advanced Inspector 
    public override string ToString() => "";
  }
}
#endif
