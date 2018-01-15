namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class TweenCallback : TweenSequenceElement {
    public struct Event {
      public readonly bool playingForwards;

      public Event(bool playingForwards) { this.playingForwards = playingForwards; }
    }

    public delegate void Act(Event evt);

    readonly Act callback;

    public TweenCallback(Act callback) { this.callback = callback; }

    public float duration => 0;
    public void setRelativeTimePassed(float t, bool playingForwards) => callback(new Event(playingForwards));
  }
}