namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class Tween {
    public static TweenCallback callback(TweenCallback.Act callback) => 
      new TweenCallback(callback);
  }

  /// <summary>
  /// Description about <see cref="A"/> start, end, ease, way to linearly interpolate and duration, packaged together.
  /// 
  /// Essentially a function from (time passed) -> (<see cref="A"/> value)
  /// </summary>
  public class Tween<A> {
    public readonly A start, end;
    public readonly Ease ease;
    public readonly TweenLerp<A> lerp;
    public readonly float duration;

    public Tween(A start, A end, Ease ease, TweenLerp<A> lerp, float duration) {
      this.start = start;
      this.end = end;
      this.ease = ease;
      this.lerp = lerp;
      this.duration = duration;
    }

    public A eval(float timePassed) => lerp(start, end, ease(timePassed / duration));

    public override string ToString() => 
      $"{nameof(Tween)}[" +
      $"{nameof(start)}={start}, " +
      $"{nameof(end)}={end}, " +
      $"{nameof(duration)}={duration}" +
      $"]";
  }
}