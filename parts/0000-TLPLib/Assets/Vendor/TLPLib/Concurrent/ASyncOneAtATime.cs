using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /* Execute asynchronous things one at a time. Useful for wrapping not 
   * concurrent event based apis to futures. */
  public class ASyncOneAtATimeQueue<Params, Return> {
    readonly Queue<Tpl<Params, FutureImpl<Return>>> queue =
      new Queue<Tpl<Params, FutureImpl<Return>>>();

    public readonly string name;
    readonly Act<Params, Promise<Return>> doQuery;

    public ASyncOneAtATimeQueue(string name, Act<Params, Promise<Return>> doQuery) {
      this.name = name;
      this.doQuery = doQuery;
    }

    public Future<Return> query(Params parameters) {
      var executeImmediately = queue.Count == 0;

      var future = new FutureImpl<Return>();

      queue.Enqueue(F.t(parameters, future));

      if (executeImmediately) {
        takeNextFromQueue();
      }
      else {
        Log.debug(name + ".query - currently executing a query, adding to queue");
      }

      return future;
    }

    void takeNextFromQueue() {
      if (queue.Count == 0) return;

      var tpl = queue.Dequeue();
      doQueryQueue(tpl._1, tpl._2);
    }

    void scheduleDequeue(Future<Return> future) {
      future.onComplete(_ => takeNextFromQueue());
    }

    void doQueryQueue(Params parameters, FutureImpl<Return> future) {
      scheduleDequeue(future);
      doQuery(parameters, future);
    }
  }
}
