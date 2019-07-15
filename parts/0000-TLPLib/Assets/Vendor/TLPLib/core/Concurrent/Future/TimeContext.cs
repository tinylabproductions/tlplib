using System;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface ITimeContext {
    Duration passedSinceStartup { get; }
    Coroutine after(Duration duration, Action act, string name = null);
  }

  public static class TimeContextExts {
    public static ITimeContext orDefault(this ITimeContext tc) => tc ?? TimeContext.DEFAULT;

    /* action should return true if it wants to keep running. */
    public static Coroutine every(
      this ITimeContext tc, Duration duration, Func<bool> action, string name = null
    ) {
      var cr = new TimeContextEveryDurationCoroutine();
      void repeatingInvoke() {
        var keepRunning = action();
        if (keepRunning) cr.current = tc.after(duration, repeatingInvoke, name);
        else cr.stop();
      }
      cr.current = tc.after(duration, repeatingInvoke, name);
      return cr;
    }

    public static Coroutine every(
      this ITimeContext tc, Duration duration, Action action, string name = null
    ) => tc.every(duration, () => { action(); return true; }, name);
  }

  class TimeContextEveryDurationCoroutine : CustomYieldInstruction, Coroutine {
    public event Action onFinish;
    public bool finished { get; private set; }
    public override bool keepWaiting => !finished;

    public Coroutine current;

    public void Dispose() {
      if (!finished) {
        current?.Dispose();
        finished = true;
        onFinish?.Invoke();
      }
    }
  }

  public class RealTimeButPauseWhenAdIsShowing : ITimeContext {
    public static readonly RealTimeButPauseWhenAdIsShowing instance = new RealTimeButPauseWhenAdIsShowing();

    readonly IRxRef<bool> externalPause;
    float totalSecondsPaused, totalSecondsPassed;
    int lastFrameCalculated;
    bool isPaused;

    /// <summary>
    /// This class calculates realtimeSinceStartup,
    /// but excludes time intervals when an ad is showing or application is paused
    ///
    /// on android - interstitials usually run on a separate activity (application gets paused/resumed automatically)
    /// on IOS and some android ad networks - application does not get paused, so we need to call `setPaused` ourselves
    /// </summary>
    RealTimeButPauseWhenAdIsShowing() {
      var pauseStarted = Time.realtimeSinceStartup;
      externalPause = RxRef.a(false);
      ASync.onAppPause.toRxVal(false).zip(externalPause, F.or2).subscribeWithoutEmit(
        NeverDisposeDisposableTracker.instance,
        paused => {
          isPaused = paused;
          if (paused) {
            pauseStarted = Time.realtimeSinceStartup;
          }
          else {
            var secondsPaused = Time.realtimeSinceStartup - pauseStarted;
            totalSecondsPaused += secondsPaused;
          }
        }
      );
    }

    public float passed { get {
      var curFrame = Time.frameCount;
      if (lastFrameCalculated != curFrame) {
        lastFrameCalculated = curFrame;
        if (!isPaused) totalSecondsPassed = Time.realtimeSinceStartup;
      }
      return totalSecondsPassed - totalSecondsPaused;
    } }

    public void setPaused(bool paused) => externalPause.value = paused;

    public Duration passedSinceStartup => Duration.fromSeconds(passed);
    public Coroutine after(Duration duration, Action act, string name = null) =>
      ASync.WithDelay(duration, act, timeContext: this);
  }

  public class TimeContext : ITimeContext {
    public static readonly TimeContext
      playMode = new TimeContext(() => Duration.fromSeconds(Time.time)),
      unscaledTime = new TimeContext(() => Duration.fromSeconds(Time.unscaledTime)),
      fixedTime = new TimeContext(() => Duration.fromSeconds(Time.fixedTime)),
      realTime = new TimeContext(() => Duration.fromSeconds(Time.realtimeSinceStartup));

    public static readonly ITimeContext
      realTimeButPauseWhenAdIsShowing = RealTimeButPauseWhenAdIsShowing.instance;

    public static ITimeContext DEFAULT => playMode;

    readonly Func<Duration> _passedSinceStartup;
    readonly MonoBehaviour maybeBehaviour;

    public TimeContext(Func<Duration> passedSinceStartup, MonoBehaviour behaviour = null) {
      _passedSinceStartup = passedSinceStartup;
      maybeBehaviour = behaviour;
    }

    public TimeContext withBehaviour(MonoBehaviour behaviour) =>
      new TimeContext(_passedSinceStartup, behaviour);

    public Duration passedSinceStartup => _passedSinceStartup();

    public Coroutine after(Duration duration, Action act, string name) =>
      ASync.WithDelay(duration, act, behaviour: maybeBehaviour, timeContext: this);
  }
}