using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.dispose {
  public class GameObjectDisposeTracker : MonoBehaviour, IMB_OnDestroy, IDisposableTracker {
    readonly LazyVal<DisposableTracker> tracker;
    public int trackedCount => tracker.strict.trackedCount;
    public IEnumerable<TrackedDisposable> trackedDisposables => tracker.strict.trackedDisposables;

    public GameObjectDisposeTracker() {
      tracker = F.lazy(() => new DisposableTracker(
        // ReSharper disable ExplicitCallerInfoArgument
        callerFilePath: Log.d.isDebug() ? gameObject.transform.debugPath() : gameObject.name,
        callerMemberName: nameof(GameObjectDisposeTracker),
        callerLineNumber: -1
        // ReSharper restore ExplicitCallerInfoArgument
      ));
    }

    public void OnDestroy() => Dispose();

    public void Dispose() {
      foreach (var t in tracker.value)
        t.Dispose();
    }

    public void track(
      IDisposable a,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => tracker.strict.track(
      a,
      // ReSharper disable ExplicitCallerInfoArgument
      callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
      // ReSharper restore ExplicitCallerInfoArgument
    );
  }

  public class GameObjectOnDisableDisposeTracker : GameObjectDisposeTracker, IMB_OnDisable {
    public void OnDisable() => Dispose();
  }

  public static class GameObjectDisposeTrackerOps {
    public static IDisposableTracker asDisposableTracker(this GameObject o) =>
      o.EnsureComponent<GameObjectDisposeTracker>();

    public static IDisposableTracker asOnDisableDisposableTracker(this GameObject o) =>
      o.EnsureComponent<GameObjectOnDisableDisposeTracker>();
  }
}