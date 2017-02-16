using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /* Execute asynchronous things one at a time. Useful for wrapping not 
   * concurrent event based apis to futures. */
  public static class ASyncNAtATimeQueue {
    public static ASyncNAtATimeQueue<Params, Return> a<Params, Return>(
      Fn<Params, Future<Return>> execute, ushort maxTasks = 1
    ) => new ASyncNAtATimeQueue<Params,Return>(maxTasks, execute);

    public static ASyncNAtATimeQueue<Params, Return> a<Params, Return>(
      Act<Params, Promise<Return>> execute, ushort maxTasks = 1
    ) => new ASyncNAtATimeQueue<Params,Return>(
      maxTasks, 
      p => Future<Return>.async(promise => execute(p, promise))
    );
  }

  public class ASyncNAtATimeQueue<Params, Return> {
    struct QueueEntry {
      public readonly Params p;
      public readonly Promise<Return> promise;

      public QueueEntry(Params p, Promise<Return> promise) {
        this.p = p;
        this.promise = promise;
      }
    }

    readonly Queue<QueueEntry> queue = new Queue<QueueEntry>();
    readonly uint maxTasks;
    readonly Fn<Params, Future<Return>> execute;

    public uint running { get; private set; }
    public uint queued => (uint) queue.Count;

    public ASyncNAtATimeQueue(uint maxTasks, Fn<Params, Future<Return>> execute) {
      this.maxTasks = maxTasks;
      this.execute = execute;
    }

    public Future<Return> enqueue(Params p) {
      if (running < maxTasks) return runTask(p);

      Promise<Return> promise;
      var f = Future.async(out promise);
      queue.Enqueue(new QueueEntry(p, promise));
      return f;
    }

    void taskCompleted() {
      running--;
      if (queued == 0) return;
      var entry = queue.Dequeue();
      runTask(entry.p).onComplete(entry.promise.complete);
    }

    Future<Return> runTask(Params p) {
      running++;
      var f = execute(p);
      f.onComplete(_ => taskCompleted());
      return f;
    }
  }
}
