using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  class ASyncHelperBehaviour : MonoBehaviour {
    /* Actions that need to be executed in the main thread. */
    private static readonly LinkedList<Act> mainThreadActions = new LinkedList<Act>();

    public void onMainThread(Act action) {
      lock (mainThreadActions) mainThreadActions.AddLast(action);
    }

    public IObservable<bool> onPause { get { return _onPause; } }
    private readonly Subject<bool> _onPause = new Subject<bool>();

    public IObservable<Unit> onQuit { get { return _onQuit; } }
    private readonly Subject<Unit> _onQuit = new Subject<Unit>();

    internal void Update() {
      if (mainThreadActions.isEmpty()) return;
      foreach (var action in mainThreadActions) action();
      mainThreadActions.Clear();
    }

    internal void OnApplicationPause(bool paused) {
      _onPause.push(paused);
    }

    internal void OnApplicationQuit() {
      _onQuit.push(F.unit);
    }
  }
}
