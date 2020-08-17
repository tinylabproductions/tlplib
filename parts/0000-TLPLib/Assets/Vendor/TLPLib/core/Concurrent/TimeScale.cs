using System;
using pzd.lib.concurrent;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public enum TimeScale {
    Realtime = 0,
    Unity = 1,
    FixedTime = 2,
    UnscaledTime = 3
  }

  public static class TimeScaleExts {
    public static float now(this TimeScale ts) {
      switch (ts) {
        case TimeScale.Realtime: return Time.realtimeSinceStartup;
        case TimeScale.Unity: return Time.time;
        case TimeScale.FixedTime: return Time.fixedTime;
        case TimeScale.UnscaledTime: return Time.unscaledTime;
        default:
          throw new ArgumentOutOfRangeException(nameof(ts), ts, null);
      }
    }

    public static float delta(this TimeScale ts) {
      switch (ts) {
        // we don't have delta time for realtime scale
        case TimeScale.Realtime:     return Time.unscaledDeltaTime;
        case TimeScale.Unity:        return Time.deltaTime;
        case TimeScale.FixedTime:    return Time.fixedDeltaTime;
        case TimeScale.UnscaledTime: return Time.unscaledDeltaTime;
        default:
          throw new ArgumentOutOfRangeException(nameof(ts), ts, null);
      }
    }

    public static ITimeContext asContext(this TimeScale ts) =>
      ts switch {
        TimeScale.Realtime => TimeContext.realTime,
        TimeScale.Unity => TimeContext.playMode,
        TimeScale.FixedTime => TimeContext.fixedTime,
        TimeScale.UnscaledTime => TimeContext.unscaledTime,
        _ => throw new ArgumentOutOfRangeException(nameof(ts), ts, null)
      };
  }
}