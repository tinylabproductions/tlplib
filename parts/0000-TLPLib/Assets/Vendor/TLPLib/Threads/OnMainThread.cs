using System;
using System.Collections.Generic;
using System.Threading;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Threads {
  /* Helper class to queue things from other threads to be ran on the main
   * thread. */
  public class OnMainThread {
    static readonly Queue<Action> actions = new Queue<Action>();
    public static readonly Thread mainThread;

    /* Initialization. */
    static OnMainThread() {
      mainThread = Thread.CurrentThread;
      ASync.EveryFrame(onUpdate);
    }

    /* Explicit initialization - we need to initialize from Unity main thread
       and this is the only way to do it.  */
    public static void init() { }

    /* Run the given action in the main thread. */
    public static void run(Action action, bool runNowIfOnMainThread=true) {
      if (Thread.CurrentThread == mainThread && runNowIfOnMainThread) action();
      else lock (actions) { actions.Enqueue(action); }
    }

    private static bool onUpdate() {
      while (true) {
        Action current;
        lock (actions) {
          if (actions.Count == 0) {
            break;
          }
          current = actions.Dequeue();
        }
        try { current(); }
        catch (Exception e) { Log.error(e); }
      }
      return true;
    }
  }
}
