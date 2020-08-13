using System;
using pzd.lib.concurrent;
using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class AsyncOperationHandleExts {
    public static Future<AsyncOperationHandle<A>> toFuture<A>(this AsyncOperationHandle<A> handle) {
      if (handle.IsDone) {
        return Future.successful(handle);
      }
      else {
        var f = Future.async<AsyncOperationHandle<A>>(out var promise);
        handle.Completed += h => promise.complete(h);
        return f;
      }
    }
    
    public static Future<AsyncOperationHandle> toFuture(this AsyncOperationHandle handle) {
      if (handle.IsDone) {
        return Future.successful(handle);
      }
      else {
        var f = Future.async<AsyncOperationHandle>(out var promise);
        handle.Completed += h => promise.complete(h);
        return f;
      }
    }

    public static Try<A> toTry<A>(this AsyncOperationHandle<A> handle) =>
      handle.Status switch {
        AsyncOperationStatus.None => new Exception("Handle is not completed!"),
        AsyncOperationStatus.Succeeded => handle.Result,
        AsyncOperationStatus.Failed => handle.OperationException,
        _ => throw new ArgumentOutOfRangeException()
      };

    public static Either<Exception, A> toEither<A>(this AsyncOperationHandle<A> handle) =>
      handle.Status switch {
        AsyncOperationStatus.None => new Exception("Handle is not completed!"),
        AsyncOperationStatus.Succeeded => handle.Result,
        AsyncOperationStatus.Failed => handle.OperationException,
        _ => throw new ArgumentOutOfRangeException()
      };

    public static Try<object> toTry(this AsyncOperationHandle handle) =>
      handle.Status switch {
        AsyncOperationStatus.None => Try<object>.failed(new Exception("Handle is not completed!")),
        AsyncOperationStatus.Succeeded => Try.value(handle.Result),
        AsyncOperationStatus.Failed => Try<object>.failed(handle.OperationException),
        _ => throw new ArgumentOutOfRangeException()
      };

    public static Either<Exception, object> toEither(this AsyncOperationHandle handle) =>
      handle.Status switch {
        AsyncOperationStatus.None => Either<Exception, object>.Left(new Exception("Handle is not completed!")),
        AsyncOperationStatus.Succeeded => Either<Exception, object>.Right(handle.Result),
        AsyncOperationStatus.Failed => Either<Exception, object>.Left(handle.OperationException),
        _ => throw new ArgumentOutOfRangeException()
      };
  }
}