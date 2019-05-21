using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Components.debug;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.dispose {
  [Record] public partial struct TrackedDisposable : IStr {
    public readonly IDisposable disposable;
    public readonly CallerData caller;

    public string asString() => caller.asString();
  }

  [PublicAPI] public interface IDisposableTracker : IDisposable {
    void track(
      IDisposable a,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    );

    int trackedCount { get; }
    IEnumerable<TrackedDisposable> trackedDisposables { get; }
  }

  public static class IDisposableTrackerExts {
    [PublicAPI] public static void track(
      this IDisposableTracker tracker,
      Action a,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => tracker.track(
      new Subscription(a),
      // ReSharper disable ExplicitCallerInfoArgument
      callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
      // ReSharper restore ExplicitCallerInfoArgument
    );
  }
  
  [PublicAPI] public class DisposableTracker : IDisposableTracker {
    readonly CallerData creator;
    
    readonly List<TrackedDisposable> list = new List<TrackedDisposable>();
    
    public int trackedCount => list.Count;
    public IEnumerable<TrackedDisposable> trackedDisposables => list;

    static readonly LazyVal<ILog> lazyLog = F.lazy(() => Log.d.withScope(nameof(DisposableTracker)));

    public DisposableTracker(
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      creator = new CallerData(callerMemberName, callerFilePath, callerLineNumber);
      var log = lazyLog.strict;
      if (log.isDebug()) log.debug($"Creating tracker at {creator}");
    }

    public void track(
      IDisposable a,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => list.Add(new TrackedDisposable(a, new CallerData(callerMemberName, callerFilePath, callerLineNumber)));

    public int count => list.Count;

    public void Dispose() {
      var log = lazyLog.strict;
      if (log.isDebug()) log.debug($"Disposing tracker at {creator}");
      foreach (var a in list) a.disposable.Dispose();
      list.Clear();
      list.Capacity = 0;
    }
  }

  /// <summary>
  /// Used when we are sure that we never want to clean the subscription automatically
  /// (for example in Observable operations).
  /// </summary>
  public class NoOpDisposableTracker : IDisposableTracker {
    public static readonly IDisposableTracker instance = new NoOpDisposableTracker();
    NoOpDisposableTracker() {}

    public void track(
      IDisposable a,
      string callerMemberName,
      string callerFilePath,
      int callerLineNumber
    ) {}

    public int trackedCount => 0;
    public IEnumerable<TrackedDisposable> trackedDisposables => Enumerable.Empty<TrackedDisposable>();
    public void Dispose() {}
  }

  /// <summary>
  /// Tracker that allows you to register a subscription to be kept forever.
  ///
  /// This should only be used for things that should never go out
  /// </summary>
  public class NeverDisposeDisposableTracker : IDisposableTracker {
    public static readonly IDisposableTracker instance = new NeverDisposeDisposableTracker();

    readonly List<TrackedDisposable> list = new List<TrackedDisposable>();
    public int trackedCount => list.Count;
    public IEnumerable<TrackedDisposable> trackedDisposables => list;

    NeverDisposeDisposableTracker() {
#if UNITY_EDITOR
      var go = new UnityEngine.GameObject(nameof(NeverDisposeDisposableTracker));
      go.exposeToInspector(this, nameof(trackedCount), _ => _.trackedCount);
      go.exposeToInspector(this, nameof(list), _ => _.list.Select(d => d.asString()).mkString("\n"));
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