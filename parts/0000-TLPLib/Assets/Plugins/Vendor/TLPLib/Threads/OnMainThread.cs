using System;
using System.Collections.Generic;
using System.Threading;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Threads {
  /* Helper class to queue things from other threads to be ran on the main
   * thread. */
  public class OnMainThread {
    static readonly Queue<Action> actions = new Queue<Action>();
    public static readonly Thread mainThread;

    /* Initialization. */
    static OnMainThread() {
      mainThread = Thread.CurrentThread;
#if UNITY_EDITOR
      if (Application.isPlaying) ASync.EveryFrame(onUpdate);
      else EditorApplication.update += () => onUpdate();
#else
      ASync.EveryFrame(onUpdate);
#endif
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod]
    // We can't add these attributes to constructor
    static void init() { }

    /* Run the given action in the main thread. */
    public static void run(Action action, bool runNowIfOnMainThread=true) {
      if (Thread.CurrentThread == mainThread && runNowIfOnMainThread) action();
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
