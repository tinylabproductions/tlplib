using System;
using com.tinylabproductions.TLPLib.Data;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface ITimeContext {
    Duration passedSinceStartup { get; }
    Coroutine after(Duration duration, Action act, string name = null);
  }

  public static class TimeContextExts {
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

  public static class TimeContext {
    public static ITimeContext DEFAULT => PlayModeTimeContext.instance;
    public static ITimeContext orDefault(this ITimeContext tc) => tc ?? DEFAULT;
  }

  public class PlayModeTimeContext : ITimeContext {
    public static readonly PlayModeTimeContext instance = new PlayModeTimeContext();
    PlayModeTimeContext() {}

    public Duration passedSinceStartup => Duration.fromSeconds(Time.time);

    public Coroutine after(Duration duration, Action act, string name) =>
      ASync.WithDelay(duration.seconds, act, timeScale: TimeScale.Unity);
  }

  public class UnscaledTimeContext : ITimeContext {
    public static readonly UnscaledTimeContext instance = new UnscaledTimeContext();
    UnscaledTimeContext() {}

    public Duration passedSinceStartup => Duration.fromSeconds(Time.unscaledTime);

    public Coroutine after(Duration duration, Action act, string name) =>
      ASync.WithDelay(duration.seconds, act, timeScale: TimeScale.UnscaledTime);
  }

  public class FixedTimeContext : ITimeContext {
    public static readonly FixedTimeContext instance = new FixedTimeContext();
    FixedTimeContext() {}

    public Duration passedSinceStartup => Duration.fromSeconds(Time.fixedTime);

    public Coroutine after(Duration duration, Action act, string name) =>
      ASync.WithDelay(duration.seconds, act, timeScale: TimeScale.FixedTime);
  }

  public class RealTimeContext : ITimeContext {
    public static readonly RealTimeContext instance = new RealTimeContext();
    RealTimeContext() {}

    public Duration passedSinceStartup => Duration.fromSeconds(Time.realtimeSinceStartup);

    public Coroutine after(Duration duration, Action act, string name) =>
      ASync.WithDelay(duration.seconds, act, timeScale: TimeScale.Realtime);
  }
}