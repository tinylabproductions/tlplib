using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;

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
  public sealed class Tween<A> {
    [PublicAPI] public readonly A start, end;
    [PublicAPI] public readonly Ease ease;
    [PublicAPI] public readonly TweenLerp<A> lerp;
    [PublicAPI] public readonly float duration;

    public Tween(A start, A end, Ease ease, TweenLerp<A> lerp, float duration) {
      if (duration < 0) {
        if (Log.d.isWarn()) Log.d.warn($"Got tween duration < 0, forcing to 0!");
        duration = 0;
      }
      
      this.start = start;
      this.end = end;
      this.ease = ease;
      this.lerp = lerp;
      this.duration = duration;
    }

    public A eval(float timePassed, bool playingForwards) =>
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      duration == 0
      ? (playingForwards ? end : start)
      : lerp(start, end, ease(timePassed / duration));

    public override string ToString() =>
      $"{nameof(Tween)}[from {start} to {end} over {duration}s]";
  }
}