using System;
using System.Threading;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Threads {
  public static class ThreadPooled {
    /* Loop `count` times, return when done. */
    public static void loop(int count, Act<int> eachIndex) {
      var finished = 0;
      var error = F.none<Exception>();
      for (var idx = 0; idx < count; idx++) {
        var localIdx = idx;
        ThreadPool.QueueUserWorkItem(_ => {
          try {
            eachIndex(localIdx);
            Interlocked.Increment(ref finished);
          }
          catch (Exception e) { error = F.some(e); }
        });
      }
      // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
      while (finished < count && error.isEmpty) Thread.Sleep(0);
      error.each(F.doThrow);
    }

    /* As #loop, but uses Future API. */
    public static Future<Unit> loopSyncF(int count, Act<int> eachIndex) {
      loop(count, eachIndex);
      return Future.successfulUnit;
    }

    /* Loop `count` times, completes the future asynchronously in main thread when done. 
     * 
     * Adds 1 frame time because calling from other threads back to unity requires going 
     * through Update(). */
    public static Future<Unit> loopF(int count, Act<int> eachIndex) {
      return Future.a<Unit>(promise => {
        var finished = 0;
        for (var idx = 0; idx < count; idx++) {
          var localIdx = idx;
          ThreadPool.QueueUserWorkItem(_ => {
            try {
              eachIndex(localIdx);
              if (Interlocked.Increment(ref finished) == count)
                ASync.OnMainThread(() => promise.tryCompleteSuccess(F.unit));
            }
            catch (Exception e) {
              ASync.OnMainThread(() => promise.tryCompleteError(e));
            }
          });
        }
      });
    }
  }
}
