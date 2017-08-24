using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public enum TweenTime : byte {
    OnUpdate, OnUpdateUnscaled, OnLateUpdate, OnLateUpdateUnscaled, OnFixedUpdate
  }

  public class TweenManager {
    public struct Loop {
      public enum Mode { Normal, YoYo }

      public const uint 
        TIMES_FOREVER = 0,
        TIMES_SINGLE = 1;

      public uint times_;
      public readonly Mode mode;

      public bool shouldLoop(uint currentIteration) => isForever || currentIteration < times_ - 1;
      public bool isForever => times_ == TIMES_FOREVER;

      public Loop(uint times, Mode mode) {
        times_ = times;
        this.mode = mode;
      }

      public static Loop forever(Mode mode = Mode.Normal) => new Loop(TIMES_FOREVER, mode);
      public static Loop single => new Loop(TIMES_SINGLE, Mode.Normal);
      public static Loop times(uint times, Mode mode = Mode.Normal) => new Loop(times, mode);
    }

    public readonly ITweenSequence sequence;
    public readonly TweenTime time;

    public event TweenCallback.Act onStart_, onEnd_;

    public float timescale = 1;
    public bool forwards = true;
    public Loop looping;
    public uint currentIteration;

    // TODO: implement me: loop(times, forever, yoyo)
    // notice: looping needs to take into account that some duration might have passed in the 
    // new iteration
    public TweenManager(ITweenSequence sequence, TweenTime time, Loop looping) {
      this.sequence = sequence;
      this.time = time;
      this.looping = looping;
    }

    public void update(float deltaTime) {
      if (!forwards) deltaTime *= -1;
      deltaTime *= timescale;

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return;

      if (forwards && sequence.isAtZero() || !forwards && sequence.isAtDuration()) {
        onStart_?.Invoke(forwards);
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
          onEnd_?.Invoke(forwards);
          stop();
        }
      }
    }

    public TweenManager onStart(TweenCallback.Act act) {
      onStart_ += act;
      return this;
    }

    public TweenManager onEnd(TweenCallback.Act act) {
      onEnd_ += act;
      return this;
    }

    /// <summary>Plays a tween from the start/end.</summary>
    public TweenManager play(bool forwards = true) {
      resume(forwards);
      return rewind();
    }

    /// <summary>Resumes playback from the last position, changing the direction.</summary>
    public TweenManager resume(bool forwards) {
      this.forwards = forwards;
      return resume();
    }

    /// <summary>Resumes playback from the last position.</summary>
    public TweenManager resume() {
      TweenManagerRunner.instance.add(this);
      return this;
    }

    /// <summary>Stops playback of the tween</summary>
    public TweenManager stop() {
      TweenManagerRunner.instance.remove(this);
      return this;
    }

    public TweenManager reverse() {
      forwards = !forwards;
      return this;
    }

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
  }

  public class TweenManagerRunner : MonoBehaviour, IMB_Update, IMB_FixedUpdate, IMB_LateUpdate {
    public static readonly TweenManagerRunner instance;

    static TweenManagerRunner() {
      if (instance == null) {
        var go = new GameObject(nameof(TweenManagerRunner));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<TweenManagerRunner>();
      }
    }

    class Tweens {
      readonly HashSet<TweenManager> 
        current = new HashSet<TweenManager>(), 
        toAdd = new HashSet<TweenManager>(), 
        toRemove = new HashSet<TweenManager>();

      bool running;

      public void add(TweenManager tm) {
        if (running) {
          // If we just stopped, but immediatelly restarted, just delete the pending removal.
          if (!toRemove.Remove(tm))
            // Otherwise schedule for addition.
            toAdd.Add(tm);
        }
        else {
          current.Add(tm);
        }
      }

      public void remove(TweenManager tm) {
        if (running) {
          if (!toAdd.Remove(tm))
            toRemove.Add(tm);
        }
        else {
          current.Remove(tm);
        }
      }

      public void runOn(float deltaTime) {
        try {
          running = true;
          foreach (var t in current)
            t.update(deltaTime);
        }
        finally {
          running = false;

          if (toRemove.Count > 0) {
            foreach (var tween in toRemove)
              current.Remove(tween);
            toRemove.Clear();
          }

          if (toAdd.Count > 0) {
            foreach (var tweenToAdd in toAdd)
              current.Add(tweenToAdd);
            toAdd.Clear();
          }
        }
      }
    }

    readonly Tweens 
      onUpdate = new Tweens(),
      onUpdateUnscaled = new Tweens(),
      onFixedUpdate = new Tweens(),
      onLateUpdate = new Tweens(),
      onLateUpdateUnscaled = new Tweens();

    TweenManagerRunner() { }

    public void Update() {
      onUpdate.runOn(Time.deltaTime);
      onUpdateUnscaled.runOn(Time.unscaledDeltaTime);
    }

    public void LateUpdate() {
      onLateUpdate.runOn(Time.deltaTime);
      onLateUpdateUnscaled.runOn(Time.unscaledDeltaTime);
    }

    public void FixedUpdate() {
      onFixedUpdate.runOn(Time.fixedDeltaTime);
    }

    public void add(TweenManager tweenManager) => 
      lookupSet(tweenManager.time).add(tweenManager);

    public void remove(TweenManager tweenManager) => 
      lookupSet(tweenManager.time).remove(tweenManager);

    Tweens lookupSet(TweenTime time) {
      switch (time) {
        case TweenTime.OnUpdate: return onUpdate;
        case TweenTime.OnUpdateUnscaled: return onUpdateUnscaled;
        case TweenTime.OnLateUpdate: return onLateUpdate;
        case TweenTime.OnLateUpdateUnscaled: return onLateUpdateUnscaled;
        case TweenTime.OnFixedUpdate: return onFixedUpdate;
        default:
          throw new ArgumentOutOfRangeException(nameof(time), time, null);
      }
    }
  }
}