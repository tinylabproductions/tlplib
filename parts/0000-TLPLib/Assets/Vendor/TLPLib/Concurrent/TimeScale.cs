using System;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public enum TimeScale { Realtime, Unity, FixedTime, UnscaledTime }

  public static class TimeScaleExts {
    public static float now(this TimeScale ts) =>
        ts == TimeScale.Realtime ? Time.realtimeSinceStartup
      : ts == TimeScale.Unity ? Time.time
      : ts == TimeScale.FixedTime ? Time.fixedTime
      : ts == TimeScale.UnscaledTime ? Time.unscaledTime
      : error<float>(ts);

    static A error<A>(TimeScale ts) =>
      F.throws<A>(new ArgumentException($"Unknown time scale: '{ts}'"));
  }
}