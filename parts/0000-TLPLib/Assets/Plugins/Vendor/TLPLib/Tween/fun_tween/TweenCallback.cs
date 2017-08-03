using System;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class TweenCallback : TweenSequenceElement {
    readonly Action callback;

    public TweenCallback(Action callback) { this.callback = callback; }

    public float duration => 0;
    public void setRelativeTimePassed(float t) => callback();
  }
}