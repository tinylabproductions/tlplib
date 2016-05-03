using System;

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
      return Future<Return>.async((p, f) => {
        theFuture.onComplete(_ => doQuery(parameters, p));
        theFuture = f;
      });
    }
  }
}
