using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
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
    readonly Dictionary<IHeapFuture<A>, Action<A>> callbacks = new Dictionary<IHeapFuture<A>, Action<A>>();

    public Action<A> this[IHeapFuture<A> ftr] {
      set { singletonAction(ftr, value); }
    }

    public void singletonAction(IHeapFuture<A> ftr, Action<A> action) {
      var value = ftr.value;
      if (value.isSome) {
        action(value.__unsafeGet);
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