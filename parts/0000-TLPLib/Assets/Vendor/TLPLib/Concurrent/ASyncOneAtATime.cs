using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /* Execute asynchronous things one at a time. Useful for wrapping not 
   * concurrent event based apis to futures. */
  public class ASyncOneAtATimeQueue<Params, Return> {
    Future<Return> theFuture = Future.successful(default(Return));

    public readonly string name;
    readonly Act<Params, Promise<Return>> doQuery;

    public ASyncOneAtATimeQueue(string name, Act<Params, Promise<Return>> doQuery) {
      this.name = name;
      this.doQuery = doQuery;
    }

    public Future<Return> query(Params parameters) {
      var future = new FutureImpl<Return>();

      theFuture.onComplete(_ => doQuery(parameters, future));
      theFuture = future;

      return future;
    }
  }
}
