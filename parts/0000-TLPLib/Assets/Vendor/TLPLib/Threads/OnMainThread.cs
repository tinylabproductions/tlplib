using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Threads {
  /// <summary>
  /// Helper class to queue things from other threads to be ran on the main thread.
  /// </summary>
  [PublicAPI] public static class OnMainThread {
    static readonly Queue<Action> actions = new Queue<Action>();
    static readonly Thread mainThread;
    public static readonly TaskScheduler mainThreadScheduler;
    
    public static bool isMainThread {
      get {
        if (mainThread == null) {
          Log.d.error(
            $"{nameof(OnMainThread)}#{nameof(isMainThread)} does not know which thread is main!"
          );
        }
        return Thread.CurrentThread == mainThread;
      }
    }

    /* Initialization. */
    static OnMainThread() {
      mainThread = Thread.CurrentThread;
      mainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
      if (Application.isPlaying) {
        // In players isPlaying is always true.
        ASync.EveryFrame(onUpdate);
      }
#if UNITY_EDITOR
      else {
        UnityEditor.EditorApplication.update += () => onUpdate();
      }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod]
    // We can't add these attributes to constructor
    static void init() {}

    /* Run the given action in the main thread. */
    public static void run(Action action, bool runNowIfOnMainThread=true) {
      if (isMainThread && runNowIfOnMainThread) {
        try { action(); }
        catch (Exception e) { Log.d.error(e); }
      }
      else lock (actions) { actions.Enqueue(action); }
    }

    static bool onUpdate() {
      while (true) {
        Action current;
        lock (actions) {
          if (actions.Count == 0) {
            break;
          }
          current = actions.Dequeue();
        }
        try { current(); }
        catch (Exception e) { Log.d.error(e); }
      }
      return true;
    }

    public static Future<Either<TaskFailed, A>> toFuture<A>(this Task<A> task) {
      var future = Future.async<Either<TaskFailed, A>>(out var promise);
      task.ContinueWith(t => {
        // exceptions thrown here get silenced
        try {
          // Task.IsCompleted documentation:
          // true if the task has completed (that is, the task is in one of the three final states: RanToCompletion,
          // Faulted, or Canceled); otherwise, false
          if (t.Status == TaskStatus.RanToCompletion) { promise.complete(t.Result); }
          else if (t.IsFaulted) { promise.complete(new TaskFailed(t.Exception)); }
          else { promise.complete(new TaskFailed(null)); }
        }
        catch (Exception e) { Log.d.error(e); }
      }, mainThreadScheduler);
      return future;
    }
  }

  [Record(GenerateToString = false), PublicAPI] public readonly partial struct TaskFailed {
    readonly AggregateException exception;

    public bool cancelled => exception == null;
    public bool failed => exception != null;

    public bool getFailure(out AggregateException e) {
      e = exception;
      return exception != null;
    }
    
    public override string ToString() => cancelled ? "TaskCancelled" : $"TaskFailed({exception})";
  }
}
