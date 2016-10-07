using com.tinylabproductions.TLPLib.Concurrent;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ASyncOperationExts {
    public static Future<AsyncOperation> toFuture(this AsyncOperation op)
      { return Future.fromBusyLoop(() => op.isDone.opt(op)); }
  }
}
