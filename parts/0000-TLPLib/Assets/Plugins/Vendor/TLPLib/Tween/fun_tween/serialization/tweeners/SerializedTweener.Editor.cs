#if UNITY_EDITOR
using UnityEngine;
namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  public abstract partial class SerializedTweener<SourceType, DestinationType, Target> {
    public override void setDuration(float dur) { _duration = dur; }
    public override Object[] getTargets() => _targets as Object[];

    // Remove this after tlplib is no longer dependant on Advanced Inspector 
    public override string ToString() => "";
  }
}
#endif
