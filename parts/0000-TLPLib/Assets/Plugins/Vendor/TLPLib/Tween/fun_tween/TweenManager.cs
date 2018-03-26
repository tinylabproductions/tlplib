using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public enum TweenTime : byte {
    OnUpdate, OnUpdateUnscaled, OnLateUpdate, OnLateUpdateUnscaled, OnFixedUpdate
  }

  /// <summary>
  /// Manages a sequence, calling its <see cref="TweenSequence.setRelativeTimePassed"/> method for you on
  /// your specified terms (for example loop 3 times, run on fixed update).
  /// </summary>
  public class TweenManager {
    public struct Loop {
      public enum Mode : byte { Normal, YoYo }

      [PublicAPI] public const uint
        TIMES_FOREVER = 0,
        TIMES_SINGLE = 1;

      [PublicAPI] public uint times_;
      [PublicAPI] public readonly Mode mode;

      [PublicAPI] public bool shouldLoop(uint currentIteration) => isForever || currentIteration < times_ - 1;
      [PublicAPI] public bool isForever => times_ == TIMES_FOREVER;

      [PublicAPI]
      public Loop(uint times, Mode mode) {
        times_ = times;
        this.mode = mode;
      }

      [PublicAPI]
      public static Loop forever(Mode mode = Mode.Normal) => new Loop(TIMES_FOREVER, mode);
      [PublicAPI]
      public static Loop foreverYoYo => new Loop(TIMES_FOREVER, Mode.YoYo);
      [PublicAPI]
      public static Loop single => new Loop(TIMES_SINGLE, Mode.Normal);
      [PublicAPI]
      public static Loop singleYoYo => new Loop(2, Mode.YoYo);
      [PublicAPI]
      public static Loop times(uint times, Mode mode = Mode.Normal) => new Loop(times, mode);
    }

    [PublicAPI] public readonly ITweenSequence sequence;
    [PublicAPI] public readonly TweenTime time;

    // These are null intentionally. We try not to create objects if they are not needed.
    ISubject<TweenCallback.Event> __onStartSubject, __onEndSubject;
    ISubject<TweenCallback.Event> onStart_ => __onStartSubject ?? (__onStartSubject = new Subject<TweenCallback.Event>());
    ISubject<TweenCallback.Event> onEnd_ => __onEndSubject ?? (__onEndSubject = new Subject<TweenCallback.Event>());

    [PublicAPI] public IObservable<TweenCallback.Event> onStart => onStart_;
    [PublicAPI] public IObservable<TweenCallback.Event> onEnd => onEnd_;
    
    [PublicAPI] public float timescale = 1;
    [PublicAPI] public bool forwards = true;
    [PublicAPI] public Loop looping;
    [PublicAPI] public uint currentIteration;

    // TODO: implement me: loop(times, forever, yoyo)
    // notice: looping needs to take into account that some duration might have passed in the
    // new iteration
    public TweenManager(ITweenSequence sequence, TweenTime time, Loop looping) {
      this.sequence = sequence;
      this.time = time;
      this.looping = looping;
    }

    [PublicAPI]
    public void update(float deltaTime) {
      if (!forwards) deltaTime *= -1;
      deltaTime *= timescale;

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return;

      if (forwards && sequence.isAtZero() || !forwards && sequence.isAtDuration()) {
        __onStartSubject?.push(new TweenCallback.Event(forwards));
      }

      var previousTime = sequence.timePassed;
      sequence.update(deltaTime);

      if (forwards && sequence.isAtDuration() || !forwards && sequence.isAtZero()) {
        if (looping.shouldLoop(currentIteration)) {
          currentIteration++;
          var unusedTime =
            Math.Abs(previousTime + deltaTime - (forwards ? sequence.duration : 0));
          switch (looping.mode) {
            case Loop.Mode.YoYo:
              reverse();
              break;
            case Loop.Mode.Normal:
              rewindTimePassed();
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
          update(unusedTime);
        }
        else {
          __onEndSubject?.push(new TweenCallback.Event(forwards));
          stop();
        }
      }
    }

    /// <summary>Plays a tween from the start/end.</summary>
    [PublicAPI]
    public TweenManager play(bool forwards = true) {
      resume(forwards);
      return rewind();
    }

    // TODO: add an option to play backwards (and test it)
    /// <summary>Plays a tween from the start at a given position.</summary>
    [PublicAPI]
    public TweenManager play(float startTime) {
      rewind();
      resume(true);
      sequence.timePassed = startTime;
      return this;
    }

    /// <summary>Resumes playback from the last position, changing the direction.</summary>
    [PublicAPI]
    public TweenManager resume(bool forwards) {
      this.forwards = forwards;
      return resume();
    }

    /// <summary>Resumes playback from the last position.</summary>
    [PublicAPI]
    public TweenManager resume() {
      TweenManagerRunner.instance.add(this);
      return this;
    }

    /// <summary>Stops playback of the tween</summary>
    [PublicAPI]
    public TweenManager stop() {
      TweenManagerRunner.instance.remove(this);
      return this;
    }

    [PublicAPI]
    public TweenManager reverse() {
      forwards = !forwards;
      return this;
    }

    [PublicAPI]
    public TweenManager rewind() {
      currentIteration = 0;
      rewindTimePassed();
      return this;
    }

    void rewindTimePassed() =>
      sequence.timePassed = forwards ? 0 : sequence.duration;
  }

  public static class TweenManagerExts {
    public static TweenManager managed(
      this ITweenSequence sequence, TweenTime time = TweenTime.OnUpdate
    ) => new TweenManager(sequence, time, TweenManager.Loop.single);

    public static TweenManager managed(
      this ITweenSequence sequence, TweenManager.Loop looping, TweenTime time = TweenTime.OnUpdate
    ) => new TweenManager(sequence, time, looping);

    public static TweenManager managed(
      this TweenSequenceElement sequence, TweenTime time = TweenTime.OnUpdate, float delay = 0
    ) => sequence.managed(TweenManager.Loop.single, time, delay);

    public static TweenManager managed(
      this TweenSequenceElement sequence, TweenManager.Loop looping, TweenTime time = TweenTime.OnUpdate,
      float delay = 0
    ) => new TweenManager(TweenSequence.single(sequence, delay), time, looping);
  }
}