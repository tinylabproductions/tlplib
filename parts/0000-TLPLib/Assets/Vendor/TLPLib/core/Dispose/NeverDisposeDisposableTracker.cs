using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Components.debug;
using pzd.lib.data;
using pzd.lib.dispose;
using pzd.lib.exts;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Dispose {
  /// <summary>
  /// Tracker that allows you to register a subscription to be kept forever.
  ///
  /// This should only be used for things that should never go out
  /// </summary>
#if UNITY_EDITOR
  [UnityEditor.InitializeOnLoad]
#endif
  public class NeverDisposeDisposableTracker : IDisposableTracker {
    public static readonly IDisposableTracker instance = new NeverDisposeDisposableTracker();

    readonly List<TrackedDisposable> list = new List<TrackedDisposable>();
    public int trackedCount => list.Count;
    public IEnumerable<TrackedDisposable> trackedDisposables => list;

    // needed for InitializeOnLoad to work
    static NeverDisposeDisposableTracker() {}

    NeverDisposeDisposableTracker() {
#if UNITY_EDITOR
      if (Application.isPlaying) {
        var go = new GameObject(nameof(NeverDisposeDisposableTracker));
        go.exposeToInspector(this, nameof(trackedCount), _ => _.trackedCount);
        go.exposeToInspector(this, nameof(list), _ => _.list.Select(d => d.asString()).mkString("\n"));
      }
#endif
    }

    public void Dispose() {}

    public void track(
      IDisposable a,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => list.Add(new TrackedDisposable(a, new CallerData(callerMemberName, callerFilePath, callerLineNumber)));
  }
}