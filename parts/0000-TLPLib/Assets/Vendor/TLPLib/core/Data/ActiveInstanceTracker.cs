using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.data.dispose;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// When you need to know about all the instances of a particular type in the scene the
  /// best way is for them to notify you about their appearance via
  /// <see cref="IMB_OnEnable"/> and <see cref="IMB_OnDisable"/> callbacks.
  /// 
  /// However usually upon those callbacks you want to run some code. And if the "manager"
  /// that you want to invoke is not there yet, you have a problem.
  /// 
  /// By using this class, you can always run code for the active instances by using <see cref="track"/>
  /// when you create the manager.
  /// 
  /// This way it does not matter whether the manager or the instances are first initialized.
  /// </summary>
  public class ActiveInstanceTracker<A> {
    readonly HashSet<A> _active = new HashSet<A>();
    readonly List<A> pendingEnables = new List<A>(), pendingDisables = new List<A>();
    
    readonly Subject<A> 
      _onEnabled = new Subject<A>(),
      _onDisabled = new Subject<A>();

    // We need to track whether we are iterating via active instances to prevent
    // concurrent modifications of a mutable data structure. Immutable data structure would
    // help us out here, but it would generate object instances on every object enable/disable,
    // which is suboptimal.
    bool iterating;

    /// It is unsafe because when we get an enumerator of a mutable data structure, it can
    /// be invalidated by mutation to that data structure. Make sure you dispose of the enumerator
    /// before any mutations can happen.
    [PublicAPI] public HashSet<A> __unsafe__active => _active;

    [PublicAPI] public void onEnable(A a) {
      if (iterating) {
        pendingEnables.Add(a);
      }
      else {
        _active.Add(a);
      }

      _onEnabled.push(a);
    }

    [PublicAPI] public void onDisable(A a) {
      if (iterating) {
        pendingDisables.Add(a);
      }
      else {
        _active.Remove(a);
      }
      
      _onDisabled.push(a);
    }

    [PublicAPI]
    public void forEach(Action<A> action) {
      try {
        iterating = true;
        foreach (var a in _active) action(a);
      }
      finally {
        iterating = false;
        foreach (var a in pendingEnables) _active.Add(a);
        foreach (var a in pendingDisables) _active.Remove(a);
        pendingEnables.Clear();
        pendingDisables.Clear();
      }
    }

    [PublicAPI]
    public void track(
      IDisposableTracker tracker, Action<A> runOnEnabled = null, Action<A> runOnDisabled = null
    ) {
      if (runOnEnabled != null) {
        // Subscribe to onEnabled before running the code on already active objects, because
        // that code can then enable additional instances.
        _onEnabled.subscribe(tracker, runOnEnabled);
        forEach(runOnEnabled);
      }

      if (runOnDisabled != null) {
        _onDisabled.subscribe(tracker, runOnDisabled);
      }
    }

    static readonly string aName = typeof(A).Name; 
    public override string ToString() => $"{_active.Count} instances of {aName}";
  }
}
