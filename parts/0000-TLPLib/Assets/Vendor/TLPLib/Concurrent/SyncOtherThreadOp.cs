using System;
using System.Threading;

namespace Assets.Vendor.TLPLib.Concurrent {
  /* Allows executing code in other threads in synchronous fashion. 
   * 
   * Operation blocks until a value can be returned or exception can be thrown. 
   */
  public class SyncOtherThreadOp<A> {
    readonly AutoResetEvent evt = new AutoResetEvent(false);
    readonly int timeoutMs;
    readonly OtherThreadExecutor<A> executor;

    Exception completedException;
    A result;

    public SyncOtherThreadOp(OtherThreadExecutor<A> executor, int timeoutMs = 1000) {
      this.executor = executor;
      this.timeoutMs = timeoutMs;
    }

    public A execute() {
      executor.execute(
        a => {
          result = a;
          evt.Set();
        },
        err => {
          completedException = err;
          evt.Set();
        }
      );
      evt.WaitOne(timeoutMs);
      if (completedException != null) throw completedException;
      return result;
    }
  }

  public interface OtherThreadExecutor<A> {
    void execute(Act<A> onSuccess, Act<Exception> onError);
  }
}
