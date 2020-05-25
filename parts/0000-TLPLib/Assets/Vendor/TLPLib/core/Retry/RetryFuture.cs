
using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Retry {
  public class RetryFuture<Error, Result> {
    readonly int retryCount;
    readonly Duration retryDelay;
    readonly Func<Future<Either<Error, Result>>> tryAction;
    readonly Func<Error, bool> shouldRetry;
    readonly TimeScale timeScale;
    readonly Promise<Either<Option<Error>, Result>> promise;
    /// <summary>
    /// Left value is last error that happened. It will be None if we cancel before first request completes.
    /// </summary>
    public readonly Future<Either<Option<Error>, Result>> future;
    
    int retries;
    IDisposable coroutine = F.emptyDisposable;
    Option<Error> lastError;

    public RetryFuture(
      int retryCount,
      Duration retryDelay,
      Func<Future<Either<Error, Result>>> tryAction,
      Func<Error, bool> shouldRetry,
      TimeScale timeScale = TimeScale.Realtime
    ) {
      future = Future.async(out promise);
      this.retryCount = retryCount;
      this.retryDelay = retryDelay;
      this.tryAction = tryAction;
      this.shouldRetry = shouldRetry;
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
      if (retries < retryCount && shouldRetry(error)) {
        retries++;
        coroutine = ASync.WithDelay(retryDelay, newRequest, timeScale: timeScale);
      }
      else {
        promise.tryComplete(lastError);
      }
    }
  }
}
