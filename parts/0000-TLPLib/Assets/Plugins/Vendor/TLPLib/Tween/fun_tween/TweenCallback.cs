using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class TweenCallback : TweenSequenceElement {
    public struct Event {
      [PublicAPI] public readonly bool playingForwards;

      public Event(bool playingForwards) { this.playingForwards = playingForwards; }
    }

    [PublicAPI] public readonly Act<Event> callback;

    public TweenCallback(Act<Event> callback) { this.callback = callback; }

    public float duration => 0;
    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) => 
      callback(new Event(playingForwards));
  }
}