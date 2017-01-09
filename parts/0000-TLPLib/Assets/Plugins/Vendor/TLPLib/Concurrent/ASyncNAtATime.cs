using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;

/**
 * This class enables running multiple (n) tasks at the same time.
 * The purpose is for example downoading multiple images in parallel.
 */

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class ASyncNAtATimeQueue<Params, Return> {
    readonly int _max;
    readonly Queue<Tpl<Params, Promise<Return>>> _queue = new Queue<Tpl<Params, Promise<Return>>>();
    readonly Act<Params, Promise<Return>> doQuery;
    int _curProcesses;

    public ASyncNAtATimeQueue(int max, Act<Params, Promise<Return>> doQuery) {
      this.doQuery = doQuery;
      _max = max;
    }

    void ProcessQueue() {
      while (_queue.Count > 0 && _curProcesses < _max) {
        _curProcesses++;
        var fut = _queue.Dequeue();
        doQuery(fut._1, fut._2);
      }
    }

    public Future<Return> query(Params parameters) {
      Promise<Return> promise;
      var future = Future<Return>.async(out promise);
      _queue.Enqueue(F.t(parameters, promise));
      ProcessQueue();
      future.onComplete(_ => {
        _curProcesses--;
        ProcessQueue();
      });
      return future;
    }
  }
}