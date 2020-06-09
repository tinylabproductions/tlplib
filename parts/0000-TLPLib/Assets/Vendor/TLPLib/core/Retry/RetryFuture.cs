using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Retry {
  public static class RetryFuture {
    public static RetryFuture<Error, Result> a<Error, Result>(
      uint retryCount,
      Duration retryDelay,
      Func<Future<Either<Error, Result>>> tryAction,
      Func<Error, bool> shouldRetry,
      ITimeContext timeContext = null
    ) => new RetryFuture<Error, Result>(
      retryCount: retryCount, retryDelay: retryDelay, tryAction: tryAction, shouldRetry: shouldRetry, 
      timeContext: timeContext
    );
  }
  
  [PublicAPI] public partial class RetryFuture<Error, Result> {
    [Record] public partial class ErrorResult {
      public readonly Option<Error> maybeError;
      public readonly bool canceledByUser;
    }
    
    readonly uint retryCount;
    readonly Duration retryDelay;
    readonly Func<Future<Either<Error, Result>>> tryAction;
    readonly Func<Error, bool> shouldRetry;
    readonly ITimeContext timeContext;
    readonly Promise<Either<ErrorResult, Result>> promise;
    
    public readonly Future<Either<ErrorResult, Result>> future;
    
    uint retries;
    IDisposable coroutine = F.emptyDisposable;
    Option<Error> lastError;

    public RetryFuture(
      uint retryCount,
      Duration retryDelay,
      Func<Future<Either<Error, Result>>> tryAction,
      Func<Error, bool> shouldRetry,
      ITimeContext timeContext = null
    ) {
      future = Future.async(out promise);
      this.retryCount = retryCount;
      this.retryDelay = retryDelay;
      this.tryAction = tryAction;
      this.shouldRetry = shouldRetry;
      this.timeContext = timeContext ?? TimeContext.realTime;
      newRequest();
    }

    public void cancel() {
      coroutine.Dispose();
      promise.tryComplete(new ErrorResult(lastError, canceledByUser: true));
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
        coroutine = timeContext.after(retryDelay, newRequest);
      }
      else {
        promise.tryComplete(new ErrorResult(lastError, canceledByUser: false));
      }
    }
  }
}
