using System;
using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class AsyncOperationHandleExts {
    public static Future<AsyncOperationHandle<A>> operationHandleToFuture<A>(this AsyncOperationHandle<A> handle) {
      var f = Future.async<AsyncOperationHandle<A>>(out var promise);
      handle.Completed += h => promise.complete(h);
      return f;
    }

    public static Either<Exception, A> operationHandleToEither<A>(this AsyncOperationHandle<A> handle) =>
      handle.Status switch {
        AsyncOperationStatus.None => new Exception("Handle is in status None!"),
        AsyncOperationStatus.Succeeded => handle.Result,
        AsyncOperationStatus.Failed => handle.OperationException,
        _ => throw new ArgumentOutOfRangeException()
      };
  }
}