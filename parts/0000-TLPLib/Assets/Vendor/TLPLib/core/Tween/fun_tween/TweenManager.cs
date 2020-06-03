using System;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using Sirenix.OdinInspector;
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
    [Serializable, Record(GenerateToString = false), InlineProperty, PublicAPI]
    public partial struct Loop {
      public enum Mode : byte { Normal, YoYo }

      public const uint
        TIMES_FOREVER = 0,
        TIMES_SINGLE = 1;

      #region Unity Serialized Fields

#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
      [SerializeField, PublicAccessor, HorizontalGroup, HideLabel, Tooltip("0 means loop forever")] uint _times_;
      [SerializeField, PublicAccessor, HorizontalGroup, HideLabel] Mode _mode;
      // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

      #endregion

      public override string ToString() {
        var timesS = _times_ == TIMES_FOREVER ? "forever" : _times_.ToString();
        return $"Loop({_mode} x {timesS})";
      }

      public bool shouldLoop(uint currentIteration) => isForever || currentIteration < times_ - 1;
      public bool isForever => times_ == TIMES_FOREVER;

      public static Loop forever(Mode mode = Mode.Normal) => new Loop(TIMES_FOREVER, mode);
      public static Loop foreverYoYo => new Loop(TIMES_FOREVER, Mode.YoYo);
      public static Loop single => new Loop(TIMES_SINGLE, Mode.Normal);
      public static Loop singleYoYo => new Loop(2, Mode.YoYo);
      public static Loop times(uint times, Mode mode = Mode.Normal) => new Loop(times, mode);
    }

    [PublicAPI] public readonly ITweenTimeline timeline;
    [PublicAPI] public readonly TweenTime time;

    IDisposableTracker _tracker;
    IDisposableTracker tracker => _tracker ??= new DisposableTracker();

    // These are null intentionally. We try not to create objects if they are not needed.
    ISubject<TweenCallback.Event> __onStartSubject, __onEndSubject;

    [PublicAPI] public IRxObservable<TweenCallback.Event> onStart => 
      __onStartSubject ??= new Subject<TweenCallback.Event>();
    [PublicAPI] public IRxObservable<TweenCallback.Event> onEnd =>
      __onEndSubject ??= new Subject<TweenCallback.Event>();

    [PublicAPI] public float timescale = 1;
    [PublicAPI] public bool forwards = true;
    [PublicAPI] public Loop looping;
    [PublicAPI] public uint currentIteration;
    public readonly string context;
    public readonly Option<Component> maybeParentComponent;
    
    [LazyProperty] static ILog log => Log.d.withScope(nameof(TweenManager));

    public TweenManager(
      ITweenTimeline timeline, TweenTime time, Loop looping, GameObject context = null,
      // stops playing the tween when parent component gets destroyed
      // this is a workaround, for missing OnDestroy callback
      Option<Component> maybeParentComponent = default
    ) {
      this.timeline = timeline;
      this.time = time;
      this.looping = looping;
      this.maybeParentComponent = maybeParentComponent;
      this.context = context ? fullName(context.transform) : "no context";

      string fullName(Transform t) {
        if (t == null) return "null context";
        if (t.parent == null) {
          return t.gameObject.scene.name + "/" + t.name;
        }
        return fullName(t.parent) + "/" + t.name;
      }
    }

    public bool update(float deltaTime) {
      try {
        updateWithScaledTime(deltaTime * timescale);
        return true;
      }
      catch (Exception e) {
        log.error(e);
        return false;
      }
    }

    void updateWithScaledTime(float deltaTime) {
      if (!forwards) deltaTime *= -1;

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return;

      if (
        currentIteration == 0 
        && (forwards && timeline.isAtZero() || !forwards && timeline.isAtDuration())
      ) {
        __onStartSubject?.push(new TweenCallback.Event(forwards));
      }

      var previousTime = timeline.timePassed;
      timeline.update(deltaTime);

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
          updateWithScaledTime(unusedTime);
        }
        else {
          __onEndSubject?.push(new TweenCallback.Event(forwards));
          stop();
        }
      }
    }

    [PublicAPI]
    public TweenManager addOnStartCallback(Action<TweenCallback.Event> act) {
      onStart.subscribe(tracker, act);
      return this;
    }

    [PublicAPI]
    public TweenManager addOnEndCallback(Action<TweenCallback.Event> act) {
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
      timeline.timePassed = startTime;
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
      if (TweenManagerRunner.hasActiveInstance) {
        // TweenManagerRunner.instance gets destroyed when we exit play mode
        // We don't want to create a new instance once that happens
        TweenManagerRunner.instance.remove(this);
      }
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
      this ITweenTimeline timeline, TweenTime time = TweenTime.OnUpdate
    ) => new TweenManager(timeline, time, TweenManager.Loop.single);

    [PublicAPI]
    public static TweenManager managed(
      this ITweenTimeline timeline, TweenManager.Loop looping, TweenTime time = TweenTime.OnUpdate
    ) => new TweenManager(timeline, time, looping);

    [PublicAPI]
    public static TweenManager managed(
      this TweenTimelineElement timeline, TweenTime time = TweenTime.OnUpdate, float delay = 0
    ) => timeline.managed(TweenManager.Loop.single, time, delay);

    [PublicAPI]
    public static TweenManager managed(
      this TweenTimelineElement timeline, TweenManager.Loop looping, TweenTime time = TweenTime.OnUpdate,
      float delay = 0
    ) => new TweenManager(TweenTimeline.single(timeline, delay), time, looping);
  }
}