using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using Smooth.Pools;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class LazyValExts {
    /// <summary>
    /// Create a new lazy value B, based on lazy value A.
    /// 
    /// Evaluating B will evaluate A, but evaluating A will not evaluate B. 
    /// </summary>
    public static LazyVal<B> lazyMap<A, B>(this LazyVal<A> lazy, Fn<A, B> mapper) => 
      F.lazy(() => mapper(lazy.get));
    
    /// <summary>
    /// Create a new lazy value B, based on lazy value A.
    /// 
    /// Evaluating B will evaluate A and evaluating A will evaluate B.
    /// 
    /// Projector function is called on every access, so make sure it is something lite,
    /// like a cast or field access. 
    /// </summary>
    public static LazyVal<B> project<A, B>(this LazyVal<A> lazy, Fn<A, B> projector) =>
      new ProjectedLazyVal<A, B>(lazy, projector);

    public static LazyVal<LST> upcast<MST, LST>(this LazyVal<MST> lazy) where MST : LST =>
      project(lazy, mst => (LST) mst);

    public static A getOrElse<A>(this LazyVal<A> lazy, Fn<A> orElse) =>
      lazy.isCompleted ? lazy.get : orElse();
    
    public static A getOrElse<A>(this LazyVal<A> lazy, A orElse) =>
      lazy.isCompleted ? lazy.get : orElse;

    public static A getOrNull<A>(this LazyVal<A> lazy) where A : class =>
      lazy.getOrElse((A) null);
  }

  // Not `Lazy<A>` because of `System.Lazy<A>`.
  public interface LazyVal<A> : IHeapFuture<A> {
    A get { get; }
  }
  
  public class NotReallyLazyVal<A> : LazyVal<A> {
    public A get { get; }

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
    public bool isCompleted { get; private set; }
    readonly Fn<A> initializer;
    readonly Option<Act<A>> afterInitialization;

    List<Act<A>> listeners;

    public LazyValImpl(Fn<A> initializer, Act<A> afterInitialization = null) {
      this.initializer = initializer;
      this.afterInitialization = afterInitialization.opt();
    }

    public A get { get {
      if (! isCompleted) {
        obj = initializer();
        isCompleted = true;
        onValueInited(obj);
      }
      return obj;
    } }

    #region Future
    
    public Option<A> value => isCompleted ? F.some(obj) : Option<A>.None;

    public void onComplete(Act<A> action) {
      if (isCompleted) action(obj);
      else {
        listeners = listeners ?? listenerPool.Borrow();
        listeners.Add(action);
      }
    }
    
    #endregion

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
  }

  class ProjectedLazyVal<A, B> : LazyVal<B> {
    readonly LazyVal<A> backing;
    readonly Fn<A, B> projector;

    public ProjectedLazyVal(LazyVal<A> backing, Fn<A, B> projector) {
      this.backing = backing;
      this.projector = projector;
    }

    public void onComplete(Act<B> action) => backing.onComplete(a => action(projector(a)));
    public Option<B> value => backing.value.map(projector);
    public bool isCompleted => backing.isCompleted;
    public B get => projector(backing.get);
  }
}
