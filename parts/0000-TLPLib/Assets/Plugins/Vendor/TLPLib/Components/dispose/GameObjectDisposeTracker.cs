using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.dispose {
  public class GameObjectDisposeTracker : MonoBehaviour, IMB_OnDestroy, IDisposableTracker {
    readonly List<IDisposable> disposables = new List<IDisposable>();

    public void OnDestroy() {
      foreach (var disposable in disposables) {
        disposable.Dispose();
      }
      disposables.Clear();
      disposables.Capacity = 0;
    }

    public void track(IDisposable a) => disposables.Add(a);
  }

  public static class GameObjectDisposeTrackerOps {
    public static IDisposableTracker asDisposableTracker(this GameObject o) =>
      o.EnsureComponent<GameObjectDisposeTracker>();
  }
}