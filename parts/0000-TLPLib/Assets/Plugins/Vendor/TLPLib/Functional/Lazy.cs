using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using Smooth.Pools;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class LazyValExts {
    public static LazyVal<B> map<A, B>(this LazyVal<A> lazy, Fn<A, B> mapper) => 
      F.lazy(() => mapper(lazy.get));
  }

  // Not `Lazy<A>` because of `System.Lazy<A>`.
  public interface LazyVal<A> : IHeapFuture<A> {
    bool initialized { get; }
    A get { get; }
    // For those cases where we want it happen as a side effect.
    A getM();
  }

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

    List<Act<A>> listeners;

    public LazyValImpl(Fn<A> initializer) {
      this.initializer = initializer;
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

    void onValueInited(A obj) {
      if (listeners != null) {
        foreach (var listener in listeners) listener(obj);
        listenerPool.Release(listeners);
        listeners = null;
      }
    }
    #endregion
  }
}
