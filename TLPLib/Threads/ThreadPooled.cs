using System;
using System.Threading;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Threads {
  public static class ThreadPooled {
    /* Loop `count` times, return when done. */
    public static void loop(int count, Act<int> eachIndex) {
      var finished = 0;
      for (var idx = 0; idx < count; idx++) {
        var localIdx = idx;
        ThreadPool.QueueUserWorkItem(_ => {
          eachIndex(localIdx);
          Interlocked.Increment(ref finished);
        });
      }
      // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
      while (finished < count) Thread.Sleep(0);
    }

    /* Loop `count` times, complete future when done. */
    public static Future<Unit> loopF(int count, Act<int> eachIndex) {
      return Future.a<Unit>(promise => {
        var finished = 0;
        for (var idx = 0; idx < count; idx++) {
          var localIdx = idx;
          ThreadPool.QueueUserWorkItem(_ => {
            eachIndex(localIdx);
            if (Interlocked.Increment(ref finished) == count) 
              promise.completeSuccess(F.unit);
          });
        }
      });
    }
  }
}
