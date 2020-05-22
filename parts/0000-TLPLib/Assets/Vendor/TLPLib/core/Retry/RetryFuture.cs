
using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Retry {
  public class RetryFuture<Error, Result> {
    readonly int retryCount;
    readonly float retryDelay;
    readonly Func<Future<Either<Error, Result>>> tryAction;
    readonly TimeScale timeScale;
    readonly Promise<Either<Option<Error>, Result>> promise;
    public readonly Future<Either<Option<Error>, Result>> future;
    
    int retries;
    IDisposable coroutine = F.emptyDisposable;
    Option<Error> lastError;

    public RetryFuture(
      int retryCount,
      float retryDelay,
      Func<Future<Either<Error, Result>>> tryAction,
      TimeScale timeScale = TimeScale.Realtime
    ) {
      future = Future.async(out promise);
      this.retryCount = retryCount;
      this.retryDelay = retryDelay;
      this.tryAction = tryAction;
      this.timeScale = timeScale;
      newRequest();
    }

    public void cancel() {
      coroutine.Dispose();
      promise.tryComplete(lastError);
    }

    void newRequest() {
      tryAction().onComplete(either => {
        either.voidFold(
          failure,
          result => promise.tryComplete(result)
        );
      });
    }

    void failure(Error error) {
      lastError = Some.a(error);
      if (retries < retryCount) {
        retries++;
        coroutine = ASync.WithDelay(retryDelay, newRequest, timeScale: timeScale);
      }
      else {
        promise.tryComplete(Some.a(error));
      }
    }
  }
}
