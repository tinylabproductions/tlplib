using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks {
  public abstract class SerializedTweenCallback : SerializedTweenTimelineElement {
    protected enum InvokeOn : byte { Both = 0, Forward = 1, Backward = 2 }

    protected abstract TweenCallback createCallback();

    TweenCallback _callback;
    [PublicAPI] public TweenCallback callback => _callback ?? (_callback = createCallback());
    public override void invalidate() => _callback = null;

    public override float duration => 0;
    
#if UNITY_EDITOR
    public override void setDuration(float dur) { }
    public override Object[] getTargets() { return new Object[]{};}
#endif

    public override IEnumerable<TweenTimelineElement> elements {
      get { yield return callback; }
    }

    protected static bool shouldInvoke(InvokeOn on, TweenCallback.Event evt) {
      switch (on) {
        case InvokeOn.Both: return true;
        case InvokeOn.Forward: return evt.playingForwards;
        case InvokeOn.Backward: return !evt.playingForwards;
        default: throw new Exception($"Unknown value for {nameof(InvokeOn)}: {on}");
      }
    }
  }
}