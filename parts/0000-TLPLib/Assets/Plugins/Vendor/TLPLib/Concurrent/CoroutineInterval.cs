using com.tinylabproductions.TLPLib.Data;
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

    public CoroutineInterval(Duration duration, TimeScale timeScale = TimeScale.Unity)
      : this(duration, timeScale.asContext()) {}

    public CoroutineInterval(Duration duration, ITimeContext timeContext) {
      this.timeContext = timeContext;
      startTime = timeContext.passedSinceStartup;
      endTime = startTime + duration;
    }

    public CoroutineIntervalEnumerator GetEnumerator() => new CoroutineIntervalEnumerator(this);

    public struct CoroutineIntervalEnumerator {
      readonly CoroutineInterval ci;
      Duration curTime;
      
      public CoroutineIntervalEnumerator(CoroutineInterval ci) : this() { this.ci = ci; }

      public bool MoveNext() {
        curTime = ci.timeContext.passedSinceStartup;
        return curTime <= ci.endTime;
      }

      public void Reset() { }

      public Percentage Current =>
        new Percentage(Mathf.InverseLerp(ci.startTime.seconds, ci.endTime.seconds, curTime.seconds));
    }
  }
}
