using System;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class Tween {
    public static TweenCallback callback(Act<TweenCallback.Event> callback) =>
      new TweenCallback(callback);

    public static Tween<A>.Ops ops<A>(Tween<A>.Lerp lerp, Tween<A>.Diff diff) =>
      new Tween<A>.Ops(lerp, diff);
  }

  /// <summary>
  /// Description about <see cref="A"/> start, end, ease, way to linearly interpolate and duration, packaged together.
  ///
  /// Essentially a function from (time passed) -> (<see cref="A"/> value)
  /// </summary>
  public sealed partial class Tween<A> {
    /// <summary>
    /// Knows how to linearly interpolate <see cref="A"/>. Should return start when y = 0 and end when y = 1.
    /// </summary>
    public delegate A Lerp(A start, A end, float y);
    public delegate A Diff(A a1, A a2);

    [Record]
    public partial struct Ops {
      public readonly Lerp lerp;
      public readonly Diff diff;
    }
    
    [PublicAPI] public readonly A start, end;
    [PublicAPI] public readonly bool isRelative;
    [PublicAPI] public readonly Ease ease;
    [PublicAPI] public readonly Ops ops;
    [PublicAPI] public readonly float duration;

    Lerp lerp => ops.lerp;
    Diff diff => ops.diff;

    public Tween(
      A start, A end, bool isRelative, Ease ease, Ops ops, float duration
    ) {
      if (duration < 0) {
        if (Log.d.isWarn()) Log.d.warn($"Got tween duration < 0, forcing to 0!");
        duration = 0;
      }
      
      this.start = start;
      this.end = end;
      this.isRelative = isRelative;
      this.ease = ease;
      this.ops = ops;
      this.duration = duration;
    }

    /// <summary>
    /// Evaluates a tween at a certain time.
    /// 
    /// Returns absolute value of <see cref="A"/> if the tween is absolute and a delta value
    /// between last and current invocations if the tween is relative.
    /// </summary>
    /// <param name="previousTimePassed">Timestamp of last time this method has been called.</param>
    /// <param name="timePassed">Timestamp of current time.</param>
    /// <param name="playingForwards">
    /// Whether this tween evaluating in a forward or backwards playing context. We could
    /// decide this from <see cref="previousTimePassed"/> and <see cref="timePassed"/>, however
    /// we can not when they are both equal, thus we need to be passed this from the outside.
    /// </param>
    public A eval(float previousTimePassed, float timePassed, bool playingForwards) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (duration == 0) {
        return isRelative 
          ? (playingForwards ? end : diff(start, end))
          : (playingForwards ? end : start);
      }
      else {
        A evalAt(float t) => lerp(start, end, ease(t / duration));
        
        var current = evalAt(timePassed);
        if (isRelative) {
          var previous = evalAt(previousTimePassed);
          return diff(current, previous);
        }
        else {
          return current;
        }
      }
    }

    public override string ToString() =>
      $"{nameof(Tween)}[from {start} to {end} over {duration}s]";
  }
}