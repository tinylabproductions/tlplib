using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class TweenCallback : TweenTimelineElement {
    public struct Event {
      [PublicAPI] public readonly bool playingForwards;

      public Event(bool playingForwards) { this.playingForwards = playingForwards; }
    }

    [PublicAPI] public readonly Action<Event> callback;

    public TweenCallback(Action<Event> callback) { this.callback = callback; }

    public float duration => 0;
    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) => 
      callback(new Event(playingForwards));

    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = default; 
      return false;
    }
  }
}