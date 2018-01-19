using System;
using System.Collections.Generic;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.dispose {
  public class GameObjectDisposeTracker : MonoBehaviour, IMB_OnDestroy, IDisposableTracker {
    readonly DisposableTracker tracker = new DisposableTracker();
    [Inspect] public int count => tracker.count;

    public void OnDestroy() => Dispose();
    public void track(IDisposable a) => tracker.track(a);
    public void Dispose() => tracker.Dispose();
  }

  public static class GameObjectDisposeTrackerOps {
    public static IDisposableTracker asDisposableTracker(this GameObject o) =>
      o.EnsureComponent<GameObjectDisposeTracker>();
  }
}