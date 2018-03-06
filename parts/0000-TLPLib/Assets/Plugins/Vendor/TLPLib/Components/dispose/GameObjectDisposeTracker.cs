using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.dispose {
  public class GameObjectDisposeTracker : MonoBehaviour, IMB_OnDestroy, IDisposableTracker {
    readonly DisposableTracker tracker = new DisposableTracker();
    public int trackedCount => tracker.trackedCount;
    public IEnumerable<TrackedDisposable> trackedDisposables => tracker.trackedDisposables;

    public void OnDestroy() => Dispose();
    public void Dispose() => tracker.Dispose();
    public void track(
      IDisposable a,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => tracker.track(
      a,
      // ReSharper disable ExplicitCallerInfoArgument
      callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
      // ReSharper restore ExplicitCallerInfoArgument
    );
  }

  public static class GameObjectDisposeTrackerOps {
    public static IDisposableTracker asDisposableTracker(this GameObject o) =>
      o.EnsureComponent<GameObjectDisposeTracker>();
  }
}