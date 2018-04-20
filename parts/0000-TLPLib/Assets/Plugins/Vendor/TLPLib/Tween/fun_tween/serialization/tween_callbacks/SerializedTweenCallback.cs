using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks {
  public abstract class SerializedTweenCallback : SerializedTweenSequenceElement {
    protected enum InvokeOn : byte { Both = 0, Forward = 1, Backward = 2 }
    
    [PublicAPI] public abstract TweenCallback callback { get; }
    public override float duration => 0;

    public override IEnumerable<TweenSequenceElement> elements {
      get { yield return callback; }
    }

    protected static bool shouldInvoke(InvokeOn on, TweenCallback.Event evt) {
      switch (on) {
        case InvokeOn.Both: return true;
        case InvokeOn.Forward: return evt.playingForwards;
        case InvokeOn.Backward: return !evt.playingForwards;
        default:
          Log.d.error($"Unknown value for {nameof(InvokeOn)}: {on}");
          return false;
      }
    }
  }
}