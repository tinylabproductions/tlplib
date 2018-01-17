using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.system;
using WeakReference = com.tinylabproductions.TLPLib.system.WeakReference;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxVal is an observable which has a current value.
   * 
   * Because it is immutable, the only way for it to change is if its source changes.
   **/
  public interface IRxVal<out A> : IObservable<A> {
    A value { get; }
    ISubscription subscribeWithoutEmit(IDisposableTracker tracker, Act<A> onEvent);
  }
  
  /// <summary>
  /// Reference that has a current value and is based on another <see cref="IObservable{A}"/>.
  /// 
  /// DO NOT initiate this yourself!
  /// 
  /// Use the provided operations like <see cref="RxValOps.map{A,B}"/>.
  /// </summary>
  public class RxVal<A> : IRxVal<A> {
    public delegate void SetValue(A a);
    
    public int subscribers => rxRef.subscribers;
    public A value => rxRef.value;
    
    readonly IRxRef<A> rxRef;
    
    // ReSharper disable once NotAccessedField.Local
    // This subscription is kept here to have a hard reference to the source.
    readonly ISubscription baseObservableSubscription;

    public RxVal(A initialValue, Fn<SetValue, ISubscription> subscribeToSource) {
      rxRef = RxRef.a(initialValue);
      var wr = WeakReference.a(this);
      var sub = Subscription.empty;
      sub = subscribeToSource(
        // This callback goes into the source observable callback list, therefore
        // we want to be very careful here to not capture this reference, to avoid
        // establishing a cirucular strong reference.
        //
        //                       +--------------+                               
        //        +--------------- Subscription <------------------------+      
        //        |              +---^----------+                        |      
        // +------v-----+            |                                   |      
        // | Source     -------------+     +----------+                +-|-----+
        // | observable -------------------> Callback -----------------> RxVal |
        // +------------+                  +----------+  weak          +-------+
        //                                               reference
        //
        // All the hard references should point backwards.
        a => {
          // Make sure to not capture `this`!
          var thizOpt = wr.Target;
          if (thizOpt.isSome) thizOpt.__unsafeGetValue.rxRef.value = a;
          else sub.unsubscribe();
        }
      );
      baseObservableSubscription = sub;
    }

    public ISubscription subscribe(IDisposableTracker tracker, Act<A> onEvent) =>
      rxRef.subscribe(tracker, onEvent);

    public ISubscription subscribeWithoutEmit(IDisposableTracker tracker, Act<A> onEvent) =>
      rxRef.subscribeWithoutEmit(tracker, onEvent);
  }

  public static class RxVal {
    #region Constructors

    /* Never changing RxVal. Useful for lifting values into reactive values. */
    public static IRxVal<A> a<A>(A value) => RxValStatic.a(value);
    public static IRxVal<A> cached<A>(A value) => RxValCache<A>.get(value);

    #endregion

    #region Ops

    static void subscribeToRescans<A>(
      IEnumerable<IRxVal<A>> vals, Action rescan
    ) {
      var doRescans = false;
      foreach (var rxVal in vals)
        rxVal.subscribe(_ => { if (doRescans) rescan(); });
      doRescans = true;
      rescan();
    }

    // TODO: test
    /** Convert an enum of rx values into one rx value using a traversal function. **/
    public static IRxVal<B> traverse<A, B>(
      this IEnumerable<IRxVal<A>> vals, Fn<IEnumerable<A>, B> traverse
    ) {
      IEnumerable<A> readValues() => vals.Select(v => v.value);
      var val = RxRef.a(traverse(readValues()));

      // TODO: this is probably suboptimal.
      void rescan() => val.value = traverse(readValues());

      subscribeToRescans(vals, rescan);
      return val;
    }

    /* Returns any value that satisfies the predicate. Order is not guaranteed. */
    public static IRxVal<Option<A>> anyThat<A, C>(
      this C vals, Fn<A, bool> predicate
    ) where C : IEnumerable<IRxVal<A>> {
      var val = RxRef.a(F.none<A>());
      var dict = new Dictionary<IRxVal<A>, A>();

      foreach (var rx in vals)
        rx.subscribe(a => {
          var matched = predicate(a);

          if (matched) {
            dict[rx] = a;
            if (val.value.isNone) val.value = a.some();
          }
          else {
            dict.Remove(rx);
            if (val.value.isSome) {
              val.value = 
                dict.isEmpty() 
                  ? Option<A>.None 
                  : dict[dict.Keys.First()].some();
            }
          }
        });
      return val;
    }

    public static IRxVal<Option<A>> anyThat<A>(
      this IEnumerable<IRxVal<A>> vals, Fn<A, bool> predicate
    ) => vals.anyThat<A, IEnumerable<IRxVal<A>>>(predicate);

    public static IRxVal<bool> anyOf<C>(this C vals, bool searchFor=true)
      where C : IEnumerable<IRxVal<bool>> 
    => 
      vals.anyThat<bool, C>(b => searchFor ? b : !b).map(_ => _.isSome);

    public static IRxVal<Option<A>> anyDefined<A>(
      this IEnumerable<IRxVal<Option<A>>> vals
    ) => 
      vals
      .anyThat<Option<A>, IEnumerable<IRxVal<Option<A>>>>(opt => opt.isSome)
      .map(_ => _.flatten());

    // TODO: test
    /**
     * Convert IRxVal<A> to IObservable<B>.
     *
     * Useful for converting from RxVal to event source. For example
     * ```someRxVal.map(_ => F.unit)``` would only emit one event, because
     * the backing value would still be IValueObservable.
     *
     * Thus we'd need to use ```someRxVal.toEventSource(_ => F.unit)```.
     **/
    public static IObservable<B> toEventSource<A, B>(
      this IRxVal<A> rxVal, IDisposableTracker tracker, Fn<A, B> mapper
    ) => new Observable<B>(onEvent => rxVal.subscribe(NoOpDisposableTracker.instance, v => onEvent(mapper(v))));

    public static IObservable<Unit> toEventSource<A>(this IRxVal<A> o) => 
      o.toEventSource(_ => F.unit);

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, IRxVal<B>> extractor
    ) => 
      source.flatMap(aOpt =>
        aOpt.map(extractor).map(rxVal => rxVal.map(val => val.some()))
          .getOrElse(cached(F.none<B>()))
      );

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, IRxVal<Option<B>>> extractor
    ) => 
      source.flatMap(aOpt =>
        aOpt.map(extractor).getOrElse(cached(F.none<B>()))
      );

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, Option<IRxVal<Option<B>>>> extractor
    ) => 
      source.flatMap(aOpt =>
        aOpt.flatMap(extractor).getOrElse(cached(F.none<B>()))
      );

    public static IRxVal<Option<B>> optMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, B> mapper
    ) => source.map(aOpt => aOpt.map(mapper));

    public static IRxVal<Option<A>> extract<A>(this Option<IRxVal<A>> rxOpt) => 
      rxOpt.fold(cached(F.none<A>()), val => val.map(a => a.some()));

    #endregion
  }

}