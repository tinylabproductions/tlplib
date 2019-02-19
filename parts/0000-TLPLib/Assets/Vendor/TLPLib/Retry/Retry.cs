
using System;
using com.tinylabproductions.TLPLib.Concurrent;

namespace com.tinylabproductions.TLPLib.Retry {
  public class Retry<A> {
    readonly int retryCount;
    readonly float retryDelay;
    readonly Action tryAction;
    readonly Action<A> failAction;
    readonly TimeScale timeScale;
    int retries;

    public Retry(int retryCount, float retryDelay, Action tryAction, Action<A> failAction, TimeScale timeScale = TimeScale.Realtime) {
      this.retryCount = retryCount;
      this.retryDelay = retryDelay;
      this.tryAction = tryAction;
      this.failAction = failAction;
      this.timeScale = timeScale;
    }

    public void start() {
      tryAction();
    }

    public void failure(Func<A> result) {
      if (retries < retryCount) {
        retries++;
        ASync.WithDelay(retryDelay, tryAction, timeScale: timeScale);
      }
      else {
        failAction(result());
      }
    }
  }
}
