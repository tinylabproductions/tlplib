using System;
using System.Threading;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /* Allows executing code in other threads in synchronous fashion. 
   * 
   * Operation blocks until a value can be returned or exception can be thrown. 
   */
  public static class SyncOtherThreadOp {
    public static SyncOtherThreadOp<A> a<A>(
      OtherThreadExecutor<A> executor, Duration timeout
    ) => new SyncOtherThreadOp<A>(executor, timeout);

    public static SyncOtherThreadOp<A> a<A>(
      OtherThreadExecutor<A> executor
    ) => a(executor, 1.second());
  }

  public class SyncOtherThreadOp<A> {
    readonly AutoResetEvent evt = new AutoResetEvent(false);
    readonly Duration timeout;
    readonly OtherThreadExecutor<A> executor;

    Exception completedException;
    A result;

    public SyncOtherThreadOp(OtherThreadExecutor<A> executor, Duration timeout) {
      this.executor = executor;
      this.timeout = timeout;
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
      evt.WaitOne(timeout.millis);
      if (completedException != null) throw completedException;
      return result;
    }
  }

  public interface OtherThreadExecutor<out A> {
    void execute(Act<A> onSuccess, Act<Exception> onError);
  }
}
