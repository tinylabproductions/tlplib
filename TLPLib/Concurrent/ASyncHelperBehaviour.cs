using System;
using System.Collections.Generic;
using System.Threading;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  class ASyncHelperBehaviour : MonoBehaviour {
    /* Actions that need to be executed in the main thread. */
    private static readonly LinkedList<Act> mainThreadActions = new LinkedList<Act>();

    static Thread mainThread;

    public void onMainThread(Act action) {
      lock (mainThreadActions) mainThreadActions.AddLast(action);
    }

    public bool isMainThread() {
      return mainThread == Thread.CurrentThread;
    }

    public IObservable<bool> onPause { get { return _onPause; } }
    private readonly Subject<bool> _onPause = new Subject<bool>();

    public IObservable<Unit> onQuit { get { return _onQuit; } }
    private readonly Subject<Unit> _onQuit = new Subject<Unit>();

    public IObservable<Unit> onLateUpdate { get { return _onLateUpdate; } }
    private readonly Subject<Unit> _onLateUpdate = new Subject<Unit>();

    internal void Start() {
      mainThread = Thread.CurrentThread;
    }

    internal void Update() {
      lock (mainThreadActions) {
        if (mainThreadActions.isEmpty()) return;
        while (mainThreadActions.Count != 0) {
          var action = mainThreadActions.First.Value;
          mainThreadActions.RemoveFirst();
          action();
        }
      }
    }

    internal void LateUpdate() {
      _onLateUpdate.push(F.unit);
    }

    internal void OnApplicationPause(bool paused) {
      _onPause.push(paused);
    }

    internal void OnApplicationQuit() {
      _onQuit.push(F.unit);
    }
  }
}
