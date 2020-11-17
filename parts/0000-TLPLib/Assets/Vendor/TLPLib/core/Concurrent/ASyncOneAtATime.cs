using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using pzd.lib.concurrent;
using pzd.lib.dispose;
using pzd.lib.log;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /* Execute asynchronous things one at a time. Useful for wrapping not
   * concurrent event based apis to futures. */
  public static class ASyncNAtATimeQueue {
    public static ASyncNAtATimeQueue<Params, Return> a<Params, Return>(
      Func<Params, Future<Return>> execute, ushort maxTasks = 1
    ) => new ASyncNAtATimeQueue<Params,Return>(maxTasks, execute);

    public static ASyncNAtATimeQueue<Params, Return> a<Params, Return>(
      Action<Params, Promise<Return>> execute, ushort maxTasks = 1
    ) => new ASyncNAtATimeQueue<Params,Return>(
      maxTasks,
      p => Future.async<Return>(promise => execute(p, promise))
    );
  }

  public class ASyncNAtATimeQueue<Params, Return> : IDisposable {
    readonly struct QueueEntry {
      public readonly Params p;
      public readonly Promise<Return> promise;

      public QueueEntry(Params p, Promise<Return> promise) {
        this.p = p;
        this.promise = promise;
      }
    }
    
    [LazyProperty, Implicit] static ILog log => Log.d.withScope(nameof(ASyncNAtATimeQueue));

    readonly DisposableTracker tracker = new DisposableTracker();
    readonly Queue<QueueEntry> queue = new Queue<QueueEntry>();
    readonly uint maxTasks;
    readonly Func<Params, Future<Return>> execute;

    public uint running { get; private set; }
    public uint queued => (uint) queue.Count;

    public ASyncNAtATimeQueue(uint maxTasks, Func<Params, Future<Return>> execute) {
      this.maxTasks = maxTasks;
      this.execute = execute;
    }

    public Future<Return> enqueue(Params p) {
      if (running < maxTasks) return runTask(p);

      var f = Future.async(out Promise<Return> promise);
      queue.Enqueue(new QueueEntry(p, promise));
      return f;
    }

    void taskCompleted() {
      running--;
      if (queued == 0) return;
      var entry = queue.Dequeue();
      tracker.track(runTask(entry.p).onCompleteCancellable(entry.promise.complete));
    }

    Future<Return> runTask(Params p) {
      running++;
      var f = execute(p);
      tracker.track(f.onCompleteCancellable(_ => taskCompleted()));
      return f;
    }

    public void Dispose() => tracker.Dispose();
  }
}
