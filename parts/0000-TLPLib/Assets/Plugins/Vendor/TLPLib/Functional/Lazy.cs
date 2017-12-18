using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using Smooth.Pools;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class LazyValExts {
    public static LazyVal<B> map<A, B>(this LazyVal<A> lazy, Fn<A, B> mapper) => 
      F.lazy(() => mapper(lazy.get));
  }

  /// <summary>
  /// Covariant version of <see cref="LazyVal{A}"/>.
  /// </summary>
  public interface LazyValCV<out A> : IHeapFuture<A> {
    bool initialized { get; }
    A get { get; }
    /// <summary>For those cases where we want it happen as a side effect.</summary>
    A getM();
  }
  
  // Not `Lazy<A>` because of `System.Lazy<A>`.
  public interface LazyVal<A> : LazyValCV<A>, IHeapValueFuture<A> {}

  public class NotReallyLazyVal<A> : LazyVal<A> {
    public bool initialized { get; } = true;
    public A get { get; }
    public A getM() => get;

    public NotReallyLazyVal(A get) { this.get = get; }

    #region Future
    public bool isCompleted => true;
    public Option<A> value => F.some(get);
    public void onComplete(Act<A> action) => action(get);
    #endregion
  }

  public class LazyValImpl<A> : LazyVal<A> {
    static readonly Pool<List<Act<A>>> listenerPool = ListPool<Act<A>>.Instance;

    A obj;
    public bool initialized { get; private set; }
    readonly Fn<A> initializer;
    readonly Option<Act<A>> afterInitialization;

    List<Act<A>> listeners;

    public LazyValImpl(Fn<A> initializer, Act<A> afterInitialization = null) {
      this.initializer = initializer;
      this.afterInitialization = afterInitialization.opt();
    }

    public A get { get {
      if (! initialized) {
        obj = initializer();
        initialized = true;
        onValueInited(obj);
      }
      return obj;
    } }

    public A getM() => get;

    #region Future
    public bool isCompleted => initialized;
    public Option<A> value => initialized ? F.some(obj) : Option<A>.None;

    public void onComplete(Act<A> action) {
      if (initialized) action(obj);
      else {
        listeners = listeners ?? listenerPool.Borrow();
        listeners.Add(action);
      }
    }

    void onValueInited(A a) {
      if (listeners != null) {
        foreach (var listener in listeners) listener(a);
        listenerPool.Release(listeners);
        listeners = null;
      }
      foreach (var act in afterInitialization) {
        act(a);
      }
    }
    #endregion
  }
}
