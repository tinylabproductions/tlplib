namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class TweenCallback : TweenSequenceElement {
    public delegate void Act(bool playingForwards);

    readonly Act callback;

    public TweenCallback(Act callback) { this.callback = callback; }

    public float duration => 0;
    public void setRelativeTimePassed(float t, bool playingForwards) => callback(playingForwards);
  }
}