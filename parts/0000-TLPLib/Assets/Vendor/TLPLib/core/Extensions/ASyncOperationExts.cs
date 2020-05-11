using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class ASyncOperationExts {
    public static Future<AsyncOperation> toFuture(this AsyncOperation op) => 
      Future.fromBusyLoop(() => op.isDone.opt(op));
    
    public static Future<IAsyncOperation> toFuture(this IAsyncOperation op) => 
      Future.fromBusyLoop(() => op.isDone.opt(op));

    public static IAsyncOperation wrap(this AsyncOperation op) => 
      new WrappedAsyncOperation(op);
  }
}
