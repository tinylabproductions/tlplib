using System;
using System.Collections.Generic;
using System.Threading;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Threads {
  /// <summary>
  /// Helper class to queue things from other threads to be ran on the main thread.
  /// </summary>
  public static class OnMainThread {
    static readonly Queue<Action> actions = new Queue<Action>();
    static readonly Thread mainThread;
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
      if (isMainThread && runNowIfOnMainThread) action();
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
  }
}
