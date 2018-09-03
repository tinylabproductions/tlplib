#if UNITY_EDITOR
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  public partial class SerializedTweenTimelineElement {
    public abstract void setDuration(float dur);
    public abstract Object[] getTargets();
  }
}
#endif
