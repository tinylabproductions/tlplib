using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// When you need to know about all the instances of a particular type in the scene the
  /// best way is for them to notify you about their appearance via
  /// <see cref="IMB_OnEnable"/> and <see cref="IMB_OnDisable"/> callbacks.
  /// 
  /// However usually upon those callbacks you want to run some code. And if the "manager"
  /// that you want to invoke is not there yet, you have a problem.
  /// 
  /// By using this class, you can always query the active instances when you create the manager
  /// and then the manager can subscribe to <see cref="onEnabled"/> and <see cref="onDisabled"/>
  /// callbacks to manage the instances.
  /// 
  /// This way it does not matter whether the manager or the instances are first initialized.
  /// </summary>
  public class ActiveInstanceTracker<A> {
    [PublicAPI] public ImmutableHashSet<A> active { get; private set; } = 
      ImmutableHashSet<A>.Empty;
    
    readonly Subject<A> 
      _onEnabled = new Subject<A>(),
      _onDisabled = new Subject<A>();

    [PublicAPI] public IObservable<A> onEnabled => _onEnabled;
    [PublicAPI] public IObservable<A> onDisabled => _onDisabled;

    [PublicAPI] public void onEnable(A a) {
      active = active.Add(a);
      _onEnabled.push(a);
    }

    [PublicAPI] public void onDisable(A a) {
      active = active.Remove(a);
      _onDisabled.push(a);
    }

    [PublicAPI]
    public void track(
      IDisposableTracker tracker, Act<A> runOnEnabled = null, Act<A> runOnDisabled = null
    ) {
      if (runOnEnabled != null) {
        foreach (var block in active) runOnEnabled(block);
        onEnabled.subscribe(tracker, runOnEnabled);
      }

      if (runOnDisabled != null) {
        onDisabled.subscribe(tracker, runOnDisabled);
      }
    }

    static readonly string aName = typeof(A).Name; 
    public override string ToString() => $"{active.Count} instances of {aName}";
  }
}