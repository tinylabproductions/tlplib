using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Pools;

namespace com.tinylabproductions.TLPLib.Concurrent {
  // Can't split into two interfaces and use variance because mono runtime
  // often crashes with variance.
  public interface IHeapFuture<A> {
    bool isCompleted { get; }
    void onComplete(Act<A> action);
    Option<A> value { get; }
  }

  public static class IHeapFutureExts {
    public static Future<A> asFuture<A>(this IHeapFuture<A> f) => Future.a(f);
  }

  class FutureImpl<A> : IHeapFuture<A>, Promise<A> {
    static readonly Pool<IList<Act<A>>> pool = new Pool<IList<Act<A>>>(
      () => new List<Act<A>>(), list => list.Clear()
    );

    IList<Act<A>> listeners = pool.Borrow();

    public bool isCompleted => value.isSome;
    public Option<A> value { get; private set; } = F.none<A>();

    public override string ToString() => $"{nameof(FutureImpl<A>)}({value})";

    public void complete(A v) {
      if (! tryComplete(v)) throw new IllegalStateException(
        $"Trying to complete future with \"{v}\" but it is already completed with \"{value.get}\""
      );
    }

    public bool tryComplete(A v) {
      // Cannot use fold here because of iOS AOT.
      var ret = value.isNone;
      if (ret) {
        value = F.some(v);
        // completed should be called only once
        completed(v);
      }
      return ret;
    }

    public void onComplete(Act<A> action) {
      if (value.isSome) action(value.get);
      else listeners.Add(action);
    }

    void completed(A v) {
      foreach (var action in listeners) action(v);
      listeners.Clear();
      pool.Release(listeners);
      listeners = null;
    }
  }

  public static class SingletonActionRegistry {
    /// <summary>
    /// Create a registry for type inferred from given parameter. 
    /// </summary>
    public static SingletonActionRegistry<A> forTypeOf<A>(IHeapFuture<A> a) =>
      new SingletonActionRegistry<A>();
  }
  /// <summary>
  /// Allows registering multiple callbacks to future completion, but differs from
  /// <see cref="Future{A}.onComplete"/> that this registry will only evaluate
  /// the last callback registered to it when the future completes.
  /// 
  /// This is very handy to register state that needs to be applied when <see cref="LazyVal{A}"/>
  /// is computed.
  /// 
  /// For example:
  /// <code><![CDATA[
  ///   readonly SingletonActionRegistry<IHasBuyWholeGameButton> singletonActionRegistry = 
  ///     new SingletonActionRegistry<IHasBuyWholeGameButton>();
  ///   
  ///   public void buyAllSetActive(
  ///     bool active, bool worldSelectActive
  ///   ) {
  ///     foreach (var s in screens.buyWholeGameScreens)
  ///       singletonActionRegistry.singletonAction(s, _ => _.buyAllContentActive = active);
  ///     singletonActionRegistry.singletonAction(screens.worldSelect, _ => _.buyAllContentActive = worldSelectActive);
  ///   }
  /// ]]></code> 
  /// </summary>
  public sealed class SingletonActionRegistry<A> {
    readonly Dictionary<IHeapFuture<A>, Act<A>> callbacks = new Dictionary<IHeapFuture<A>, Act<A>>();

    public Act<A> this[IHeapFuture<A> ftr] {
      set { singletonAction(ftr, value); }
    }
    
    public void singletonAction(IHeapFuture<A> ftr, Act<A> action) {
      if (ftr.isCompleted) {
        ftr.onComplete(action);
      }
      else {
        if (!callbacks.Remove(ftr)) {
          ftr.onComplete(a => futureCompleted(ftr, a));
        }
        callbacks.Add(ftr, action); 
      }
    }

    void futureCompleted(IHeapFuture<A> ftr, A a) => callbacks.a(ftr)(a);
  }
}
