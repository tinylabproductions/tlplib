using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;
using JetBrains.Annotations;
using pzd.lib.exts;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class ASyncOperationExts {
    public static Future<AsyncOperation> toFuture(this AsyncOperation op) => 
      FutureU.fromBusyLoop(() => op.isDone.opt(op));
    
    public static Future<IAsyncOperation> toFuture(this IAsyncOperation op) => 
      FutureU.fromBusyLoop(() => op.isDone.opt(op));

    public static IAsyncOperation wrap(this AsyncOperation op) => 
      new WrappedAsyncOperation(op);
  }
}
