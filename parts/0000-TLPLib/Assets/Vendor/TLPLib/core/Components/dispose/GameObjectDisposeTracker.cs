using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using pzd.lib.dispose;
using pzd.lib.log;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.dispose {
  public class GameObjectDisposeTracker : MonoBehaviour, IMB_OnDestroy, IDisposableTracker {
    [LazyProperty] static ILog log => Log.d.withScope(nameof(GameObjectDisposeTracker));

    readonly LazyVal<DisposableTracker> tracker;
    public int trackedCount => tracker.strict.trackedCount;
    public IEnumerable<TrackedDisposable> trackedDisposables => tracker.strict.trackedDisposables;

    public GameObjectDisposeTracker() {
      tracker = F.lazy(() => new DisposableTracker(
        log,
        // ReSharper disable ExplicitCallerInfoArgument
        callerFilePath: Log.d.isDebug() ? gameObject.transform.debugPath() : gameObject.name,
        callerMemberName: nameof(GameObjectDisposeTracker),
        callerLineNumber: -1
        // ReSharper restore ExplicitCallerInfoArgument
      ));
    }

    public void OnDestroy() => Dispose();

    public void Dispose() {
      if (tracker.value.valueOut(out var t))
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

  public static class GameObjectDisposeTrackerOps {
    [Obsolete(
      "Unity does not invoke OnDestroy() if the object was not previously invoked.\n" +
      "This can lead to a disposable tracker never being disposed of.\n" +
      "\n" +
      "Instead of using this please create a tracker manually and use/dispose of it yourself.\n" +
      "\n" +
      "https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDestroy.html"
    )]
    public static IDisposableTracker asDisposableTracker(this GameObject o) =>
      o.EnsureComponent<GameObjectDisposeTracker>();
  }
}