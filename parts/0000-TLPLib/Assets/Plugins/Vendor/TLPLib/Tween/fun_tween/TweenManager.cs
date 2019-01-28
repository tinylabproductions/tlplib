using System;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public enum TweenTime : byte {
    OnUpdate, OnUpdateUnscaled, OnLateUpdate, OnLateUpdateUnscaled, OnFixedUpdate
  }

  /// <summary>
  /// Manages a sequence, calling its <see cref="TweenTimeline.setRelativeTimePassed"/> method for you on
  /// your specified terms (for example loop 3 times, run on fixed update).
  /// </summary>
  public partial class TweenManager : IDisposable {
    [Serializable, Record(GenerateToString = false)]
    public partial struct Loop {
      public enum Mode : byte { Normal, YoYo }

      [PublicAPI, HideInInspector] public const uint
        TIMES_FOREVER = 0,
        TIMES_SINGLE = 1;

      #region Unity Serialized Fields

#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
      [SerializeField, PublicAccessor, Tooltip("0 means loop forever")] uint _times_;
      [SerializeField, PublicAccessor] Mode _mode;
      // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

      #endregion

      public override string ToString() {
        var timesS = _times_ == TIMES_FOREVER ? "forever" : _times_.ToString();
        return $"Loop({_mode} x {timesS})";
      }

      [PublicAPI] public bool shouldLoop(uint currentIteration) => isForever || currentIteration < times_ - 1;
      [PublicAPI] public bool isForever => times_ == TIMES_FOREVER;

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

    [PublicAPI] public readonly ITweenTimeline timeline;
    [PublicAPI] public readonly TweenTime time;
    [PublicAPI] public readonly string context;
    [PublicAPI] public readonly bool stopIfDestroyed;

    IDisposableTracker _tracker;
    IDisposableTracker tracker => _tracker ?? (_tracker = new DisposableTracker());

    // These are null intentionally. We try not to create objects if they are not needed.
    ISubject<TweenCallback.Event> __onStartSubject, __onEndSubject;

    [PublicAPI] public IObservable<TweenCallback.Event> onStart => 
      __onStartSubject ?? (__onStartSubject = new Subject<TweenCallback.Event>());
    [PublicAPI] public IObservable<TweenCallback.Event> onEnd =>
      __onEndSubject ?? (__onEndSubject = new Subject<TweenCallback.Event>());

    [PublicAPI] public float timescale = 1;
    [PublicAPI] public bool forwards = true;
    [PublicAPI] public Loop looping;
    [PublicAPI] public uint currentIteration;

    // TODO: implement me: loop(times, forever, yoyo)
    // notice: looping needs to take into account that some duration might have passed in the
    // new iteration
    public TweenManager(ITweenTimeline timeline, TweenTime time, Loop looping, string context, bool stopIfDestroyed = false) {
      this.timeline = timeline;
      this.time = time;
      this.looping = looping;
      this.context = context;
      this.stopIfDestroyed = stopIfDestroyed;
    }

    [PublicAPI]
    public bool update(float deltaTime) {
      var noErrors = true;

      if (!forwards) deltaTime *= -1;
      deltaTime *= timescale;

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return noErrors;

      if (
        currentIteration == 0 
        && (forwards && timeline.isAtZero() || !forwards && timeline.isAtDuration())
      ) {
        __onStartSubject?.push(new TweenCallback.Event(forwards));
      }

      var previousTime = timeline.timePassed;
      if (!timeline.update(deltaTime)) noErrors = false;

      if (forwards && timeline.isAtDuration() || !forwards && timeline.isAtZero()) {
        if (looping.shouldLoop(currentIteration)) {
          currentIteration++;
          var unusedTime =
            Math.Abs(previousTime + deltaTime - (forwards ? timeline.duration : 0));
          switch (looping.mode) {
            case Loop.Mode.YoYo:
              reverse();
              break;
            case Loop.Mode.Normal:
              rewindTimePassed(false);
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
          if (!update(unusedTime)) noErrors = false;
        }
        else {
          __onEndSubject?.push(new TweenCallback.Event(forwards));
          stop();
        }
      }

      return noErrors;
    }

    [PublicAPI]
    public TweenManager addOnStartCallback(Act<TweenCallback.Event> act) {
      onStart.subscribe(tracker, act);
      return this;
    }

    [PublicAPI]
    public TweenManager addOnEndCallback(Act<TweenCallback.Event> act) {
      onEnd.subscribe(tracker, act);
      return this;
    }

    /// <summary>Plays a tween from the start/end.</summary>
    [PublicAPI]
    public TweenManager play(bool forwards = true) {
      resume(forwards);
      return rewind();
    }

    /// <summary>Plays a tween from the start at a given position.</summary>
    // TODO: add an option to play backwards (and test it)
    [PublicAPI]
    public TweenManager play(float startTime) {
      rewind();
      resume(true);
      timeline.setTimePassed(startTime, true);
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
    public TweenManager rewind(bool applyEffectsForRelativeTweens = false) {
      currentIteration = 0;
      rewindTimePassed(applyEffectsForRelativeTweens);
      return this;
    }

    void rewindTimePassed(bool applyEffectsForRelativeTweens) =>
      timeline.setTimePassed(forwards ? 0 : timeline.duration, applyEffectsForRelativeTweens);

    public void Dispose() {
      stop();
      _tracker?.Dispose();
    }
  }

  public static class TweenManagerExts {
    [PublicAPI]
    public static TweenManager managed(
      this ITweenTimeline timeline, TweenTime time = TweenTime.OnUpdate,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => new TweenManager(
      timeline, time, TweenManager.Loop.single, createContext(callerMemberName, callerFilePath, callerLineNumber)
    );

    [PublicAPI]
    public static TweenManager managed(
      this ITweenTimeline timeline, TweenManager.Loop looping, TweenTime time = TweenTime.OnUpdate,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => new TweenManager(timeline, time, looping, createContext(callerMemberName, callerFilePath, callerLineNumber));

    [PublicAPI]
    public static TweenManager managed(
      this TweenTimelineElement timeline, TweenTime time = TweenTime.OnUpdate, float delay = 0,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
      // ReSharper disable ExplicitCallerInfoArgument
    ) => timeline.managed(TweenManager.Loop.single, time, delay, callerMemberName, callerFilePath, callerLineNumber);
    // ReSharper restore ExplicitCallerInfoArgument

    [PublicAPI]
    public static TweenManager managed(
      this TweenTimelineElement timeline, TweenManager.Loop looping, TweenTime time = TweenTime.OnUpdate,
      float delay = 0,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => new TweenManager(
      TweenTimeline.single(timeline, delay), time, looping,
      createContext(callerMemberName, callerFilePath, callerLineNumber)
    );

    static string createContext(string callerMemberName, string callerFilePath, int callerLineNumber) =>
      $"{callerMemberName} @ {callerFilePath}:{callerLineNumber}.";
  }
}