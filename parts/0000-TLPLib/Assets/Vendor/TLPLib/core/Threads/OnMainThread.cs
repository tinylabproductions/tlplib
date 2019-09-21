using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Threads {
  /// <summary>
  /// Helper class to queue things from other threads to be ran on the main thread.
  /// </summary>
  [PublicAPI] public static class OnMainThread {
    static readonly Queue<Action> actions = new Queue<Action>();
    static Thread mainThread;
    public static TaskScheduler mainThreadScheduler;
    
    public static bool isMainThread {
      get {
        if (mainThread == null) {
          // can't use Log.d here, because it calls isMainThread
          Debug.LogError(
            $"{nameof(OnMainThread)}#{nameof(isMainThread)} does not know which thread is main!"
          );
        }
        return Thread.CurrentThread == mainThread;
      }
    }

    // mainThread variable may not be initialized in editor when MonoBehaviour constructor gets called
    public static bool isMainThreadIgnoreUnknown => Thread.CurrentThread == mainThread;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod]
    static void init() {
      // Can't use static constructor, because it may be called from a different thread
      // init will always be called fom a main thread
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
          if (t.Status == TaskStatus.RanToCompletion) { promise.completeOrLog(t.Result); }
          else if (t.IsFaulted) { promise.completeOrLog(new TaskFailed(t.Exception)); }
          else { promise.completeOrLog(new TaskFailed(null)); }
        }
        catch (Exception e) { promise.completeOrLog(new TaskFailed(new AggregateException(e))); }
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
