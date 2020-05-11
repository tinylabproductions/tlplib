using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class AsyncOperationHandleExts {
    public static Future<AsyncOperationHandle<A>> toFuture<A>(this AsyncOperationHandle<A> handle) {
      var f = Future.async<AsyncOperationHandle<A>>(out var promise);
      handle.Completed += h => promise.complete(h);
      return f;
    } 
  }
}