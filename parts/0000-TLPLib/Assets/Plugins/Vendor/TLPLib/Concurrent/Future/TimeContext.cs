using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
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
      this ITimeContext tc, Duration duration, Fn<bool> action, string name = null
    ) {
      var cr = new TimeContextEveryDurationCoroutine();
      Action repeatingInvoke = null;
      repeatingInvoke = () => {
        var keepRunning = action();
        if (keepRunning) cr.current = tc.after(duration, repeatingInvoke, name);
        else cr.stop();
      };
      cr.current = tc.after(duration, repeatingInvoke, name);
      return cr;
    }

    public static Coroutine every(
      this ITimeContext tc, Duration duration, Action action, string name = null
    ) => tc.every(duration, () => { action(); return true; }, name);
  }

  class TimeContextEveryDurationCoroutine : Coroutine {
    public event Action onFinish;
    public bool finished { get; private set; }

    public Coroutine current;

    public void Dispose() {
      if (!finished) {
        current?.Dispose();
        finished = true;
        onFinish?.Invoke();
      }
    }
  }

  public class TimeContext : ITimeContext {
    public static readonly TimeContext 
      playMode = new TimeContext(TimeScale.Unity, () => Duration.fromSeconds(Time.time)),
      unscaledTime = new TimeContext(TimeScale.UnscaledTime, () => Duration.fromSeconds(Time.unscaledTime)),
      fixedTime = new TimeContext(TimeScale.FixedTime, () => Duration.fromSeconds(Time.fixedTime)),
      realTime = new TimeContext(TimeScale.Realtime, () => Duration.fromSeconds(Time.realtimeSinceStartup));

    public static ITimeContext DEFAULT => playMode;

    readonly TimeScale timeScale;
    readonly Fn<Duration> _passedSinceStartup;
    readonly Option<MonoBehaviour> behaviour;

    public TimeContext(
      TimeScale timeScale, Fn<Duration> passedSinceStartup, 
      Option<MonoBehaviour> behaviour = default(Option<MonoBehaviour>)
    ) {
      Option.ensureValue(ref behaviour);

      this.timeScale = timeScale;
      _passedSinceStartup = passedSinceStartup;
      this.behaviour = behaviour;
    }

    public TimeContext withBehaviour(MonoBehaviour behaviour) =>
      new TimeContext(timeScale, _passedSinceStartup, behaviour.some());

    public Duration passedSinceStartup => _passedSinceStartup();

    public Coroutine after(Duration duration, Action act, string name) =>
      ASync.WithDelay(duration.seconds, act, behaviour: behaviour.orNull(), timeScale: timeScale);
  }
}