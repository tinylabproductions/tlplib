using com.tinylabproductions.TLPLib.Data;
using pzd.lib.concurrent;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /*
    Sample usage:

    foreach (var p in new CoroutineInterval(3)) {
      // p goes from 0 to 1 linearly
      setValue(Mathf.Lerp(start, end, p.value);
      yield return null;
    }
    // inside loop never reaches exactly 1, so we need to handle this after the loop
    setValue(end);
  */
  public struct CoroutineInterval {
    readonly ITimeContext timeContext;
    readonly Duration startTime, endTime;
    readonly bool alwaysFinish;

    public CoroutineInterval(Duration duration, TimeScale timeScale = TimeScale.Unity, bool alwaysFinish = true)
      : this(duration, timeScale.asContext(), alwaysFinish) {}

    public CoroutineInterval(Duration duration, ITimeContext timeContext, bool alwaysFinish = true) {
      this.timeContext = timeContext;
      startTime = timeContext.passedSinceStartup;
      endTime = startTime + duration;
      this.alwaysFinish = alwaysFinish;
    }

    public CoroutineIntervalEnumerator GetEnumerator() => new CoroutineIntervalEnumerator(this);

    public struct CoroutineIntervalEnumerator {
      readonly CoroutineInterval ci;
      Duration curTime;
      bool finished;
      
      public CoroutineIntervalEnumerator(CoroutineInterval ci) : this() { this.ci = ci; }

      public bool MoveNext() {
        curTime = ci.timeContext.passedSinceStartup;
        if (ci.alwaysFinish) {
          var result = !finished;
          if (curTime >= ci.endTime) {
            finished = true;
          }
          return result;
        }
        else return curTime <= ci.endTime;
      }

      public Percentage Current =>
        new Percentage(Mathf.InverseLerp(ci.startTime.seconds, ci.endTime.seconds, curTime.seconds));
    }
  }
}
