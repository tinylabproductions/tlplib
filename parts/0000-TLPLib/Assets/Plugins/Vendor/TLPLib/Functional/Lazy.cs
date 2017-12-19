using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using Smooth.Pools;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class LazyValExts {
    public static LazyVal<B> map<A, B>(this LazyVal<A> lazy, Fn<A, B> mapper) => 
      F.lazy(() => mapper(lazy.get));

    /// <summary>
    /// Allows to use lazy value of more specific type where a lazy value of less specific type
    /// is required.
    /// 
    /// You would think that this should work:
    /// <code><![CDATA[
    /// worldSelect.map(_ => _.upcast<WorldSelectScreen.Init, IScreen>())
    /// ]]></code>
    /// 
    /// However, because <see cref="map{A,B}"/> creates a new lazy value, it does not propogate
    /// the initialization status. That means an underlying lazy value might be initialized, but 
    /// a mapped value would not be.
    /// </summary>
    /// <typeparam name="MST">More specific type (like Cat)</typeparam>
    /// <typeparam name="LST">Less specific type (like Animal)</typeparam>
    public static LazyVal<LST> upcast<MST, LST>(this LazyVal<MST> lazy) where MST : LST => 
      new UpcastLazyVal<MST, LST>(lazy);

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

  /// <summary>
  /// Helps to use <see cref="LazyVal{A}"/> where variance is needed.
  /// 
  /// <see cref="LazyValExts.upcast{MST,LST}"/>
  /// </summary>
  /// <typeparam name="MST">More specific type (like Cat)</typeparam>
  /// <typeparam name="LST">Less specific type (like Animal)</typeparam>
  class UpcastLazyVal<MST, LST> : LazyVal<LST> where MST : LST {
    readonly LazyVal<MST> backing;

    public UpcastLazyVal(LazyVal<MST> backing) { this.backing = backing; }

    public void onComplete(Act<LST> action) => backing.onComplete(a => action(a));
    public Option<LST> value => backing.value.map(_ => _.upcast<MST, LST>());
    public bool isCompleted => backing.isCompleted;
    public LST get => backing.get;
  }
}
