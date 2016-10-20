using com.tinylabproductions.TLPLib.Data;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /*
    Sample usage:

    foreach (var p in new CoroutineInterval(3)) {
      // p goes from 0 to 1 linearly
      yield return null;
    }
  */
  public struct CoroutineInterval {
    readonly TimeScale timeScale;
    readonly float startTime, endTime;

    public CoroutineInterval(Duration duration, TimeScale timeScale = TimeScale.Unity) {
      this.timeScale = timeScale;
      startTime = timeScale.now();
      endTime = startTime + duration.seconds;
    }

    public CoroutineIntervalEnumerator GetEnumerator() => new CoroutineIntervalEnumerator(this);

    public struct CoroutineIntervalEnumerator {
      readonly CoroutineInterval ci;
      float curTime;

      public CoroutineIntervalEnumerator(CoroutineInterval ci) : this() { this.ci = ci; }

      public bool MoveNext() {
        curTime = ci.timeScale.now();
        return curTime <= ci.endTime;
      }

      public void Reset() { }

      public Percentage Current => new Percentage(Mathf.InverseLerp(ci.startTime, ci.endTime, curTime));
    }
  }
}
