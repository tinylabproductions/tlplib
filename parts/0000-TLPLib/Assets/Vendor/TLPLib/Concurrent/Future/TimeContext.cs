using System;
using com.tinylabproductions.TLPLib.Data;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface ITimeContext {
    Duration passedSinceStartup { get; }
    void after(Duration duration, Action act);
    void afterXFrames(int framesToSkip, Action act);
  }

  public static class TimeContext {
    public static ITimeContext DEFAULT => PlayModeTimeContext.instance;
    public static ITimeContext orDefault(this ITimeContext tc) => tc ?? DEFAULT;
  }

  public class PlayModeTimeContext : ITimeContext {
    public static readonly PlayModeTimeContext instance = new PlayModeTimeContext();
    PlayModeTimeContext() {}

    public Duration passedSinceStartup => Duration.fromSeconds(Time.realtimeSinceStartup);

    public void after(Duration duration, Action act) =>
      ASync.WithDelay(duration.seconds, act);

    public void afterXFrames(int framesToSkip, Action act) =>
      ASync.AfterXFrames(framesToSkip, act);
  }
}