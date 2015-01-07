using System;
using System.Threading;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Pools;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Threads {
  public static class ThreadPooled {
    static readonly Pool<ManualResetEvent> eventPool = new Pool<ManualResetEvent>(
      () => new ManualResetEvent(false),
      e => e.Reset()
    );

    static void loop(
      int count, int chunks, Act onFinish, Act<Exception> onError, Act<int> eachIndex
    ) {
      if (chunks <= 0) chunks = Environment.ProcessorCount;
      if (chunks > count) chunks = count;

      var chunkSize = Mathf.CeilToInt((float) count / chunks);
      var finished = 0;
      for (var chunkStartIdx = 0; chunkStartIdx < count; chunkStartIdx += chunkSize) {
        var startIdx = chunkStartIdx;
        var chunkEndIdx = Mathf.Min(chunkStartIdx + chunkSize, count); // Exclusive
        ThreadPool.QueueUserWorkItem(_ => {
          for (var idx = startIdx; idx < chunkEndIdx; idx++) {
            try { eachIndex(idx); }
            catch (Exception e) { onError(e); }
          }
          if (Interlocked.Add(ref finished, chunkEndIdx - startIdx) == count) onFinish();
        });
      }
    }

    /* Loop `count` times, doing each iteration in separate work item, return when done.
     * 
     * If `chunks` <= 0, chunks = processor count.
     * If `chunks` >= count, clamps to count.
     */
    public static void loop(int count, Act<int> eachIndex, int chunks=0) {
      var evt = eventPool.Borrow();
      var error = F.none<Exception>();
      loop(
        count, chunks, () => evt.Set(), err => { error = F.some(err); evt.Set(); }, 
        eachIndex
      );
      evt.WaitOne();
      eventPool.Release(evt);
      error.each(F.doThrow);
    }

    /* As #loop, but uses Future API. */
    public static Future<Unit> loopSyncF(int count, Act<int> eachIndex, int chunks = 0) {
      loop(count, eachIndex, chunks);
      return Future.successfulUnit;
    }

    /* As #loop, but completes the future asynchronously in main thread when done. 
     * 
     * Adds 1 frame time because calling from other threads back to unity requires going 
     * through Update(). */
    public static Future<Unit> loopF(int count, Act<int> eachIndex, int chunks = 0) {
      return Future.a<Unit>(promise => loop(
        count, chunks,
        () => ASync.OnMainThread(() => { promise.tryCompleteSuccess(F.unit); }), 
        err => ASync.OnMainThread(() => { promise.tryCompleteError(err); }), 
        eachIndex
      ));
    }
  }
}
