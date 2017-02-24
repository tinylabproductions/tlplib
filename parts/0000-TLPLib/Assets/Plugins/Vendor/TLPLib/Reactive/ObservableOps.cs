using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class ObservableOps {
    public static ISubscription subscribeForOneEvent<A>(
      this IObservable<A> observable, Act<A> onEvent
    ) => observable.subscribe((a, sub) => {
      sub.unsubscribe();
      onEvent(a);
    });

    /** Maps events coming from this observable. **/
    public static IObservable<B> map<A, B>(
      this IObservable<A> o, Fn<A, B> mapper
    ) => Observable.a(ObservableOpImpls.map(o, mapper));

    #region #flatMap

    /** 
     * Maps events coming from this observable and emits all events contained 
     * in returned enumerable.
     **/
    public static IObservable<B> flatMap<A, B>(
      this IObservable<A> o, Fn<A, IEnumerable<B>> mapper
    ) => Observable.a(ObservableOpImpls.flatMap(o, mapper));

    /** 
     * Maps events coming from this observable and emits all events contained 
     * in returned observable.
     **/
    public static IObservable<B> flatMap<A, B>(
      this IObservable<A> o, Fn<A, IObservable<B>> mapper
    ) => Observable.a(ObservableOpImpls.flatMap(o, mapper));

    /** 
     * Maps events coming from this observable and emits events from returned futures.
     * 
     * Does not emit value if future completes after observable is finished.
     **/
    public static IObservable<B> flatMap<A, B>(
      this IObservable<A> o, Fn<A, Future<B>> mapper
    ) => Observable.a(ObservableOpImpls.flatMap(o, mapper));

    #endregion

    /** Only emits events that pass the predicate. **/
    public static IObservable<A> filter<A>(
      this IObservable<A> o, Fn<A, bool> predicate
    ) => Observable.a(ObservableOpImpls.filter(o, predicate));

    /** Emits first value to the future and unsubscribes. **/
    public static Future<A> toFuture<A>(this IObservable<A> o) =>
      Future<A>.async((p, f) => {
        var subscription = o.subscribe(p.complete);
        f.onComplete(_ => subscription.unsubscribe());
      });

    // Skips `count` values from the stream.
    public static IObservable<A> skip<A>(this IObservable<A> o, uint count) =>
      Observable.a(ObservableOpImpls.skip(o, count));

    // If several events are emitted per same frame, only emit last one in late update.
    // TODO: test, but how? Can't do async tests in unity.
    public static IObservable<A> oncePerFrame<A>(this IObservable<A> o) => 
      Observable.a(ObservableOpImpls.oncePerFrame(o));

    #region #zip

    public static IObservable<Tpl<A, B>> zip<A, B>(this IObservable<A> o, IObservable<B> other) => 
      Observable.a(ObservableOpImpls.zip(o, other));

    public static IObservable<Tpl<A, B, C>> zip<A, B, C>(
      this IObservable<A> o, IObservable<B> o1, IObservable<C> o2
    ) => Observable.a(ObservableOpImpls.zip(o, o1, o2));

    public static IObservable<Tpl<A, B, C, D>> zip<A, B, C, D>(
      this IObservable<A> o, IObservable<B> o1, IObservable<C> o2, IObservable<D> o3
    ) => Observable.a(ObservableOpImpls.zip(o, o1, o2, o3));

    public static IObservable<Tpl<A, B, C, D, E>> zip<A, B, C, D, E>(
      this IObservable<A> o, IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4
    ) => Observable.a(ObservableOpImpls.zip(o, o1, o2, o3, o4));

    public static IObservable<Tpl<A, A1, A2, A3, A4, A5>> zip<A, A1, A2, A3, A4, A5>(
      this IObservable<A> o, IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, 
      IObservable<A4> o4, IObservable<A5> o5
    ) => Observable.a(ObservableOpImpls.zip(o, o1, o2, o3, o4, o5));

    #endregion

    /**
     * Discards values that this observable emits, turning it into event 
     * source that does not carry data with it.
     **/
    public static IObservable<Unit> discardValue<A>(this IObservable<A> o) =>
      Observable.a(ObservableOpImpls.discardValue(o));

    /** Only emits events that return some. **/
    public static IObservable<B> collect<A, B>(
      this IObservable<A> o, Fn<A, Option<B>> collector
    ) => Observable.a(ObservableOpImpls.collect(o, collector));

    #region #buffer

    /**
      * Buffers values into a linked list of specified size. Oldest values 
      * are at the front of the buffer. Only emits `size` items at a time. When
      * new item arrives to the buffer, oldest one is removed.
      **/
    public static IObservable<ReadOnlyLinkedList<A>> buffer<A>(
      this IObservable<A> o, int size
    ) => o.buffer(size, new ObservableReadOnlyLinkedListQueue<A>());

    public static IObservable<C> buffer<A, C>(
      this IObservable<A> o, int size, IObservableQueue<A, C> queue
    ) => Observable.a(ObservableOpImpls.buffer(o, size, queue));

    #endregion

    #region #timeBuffer

    /**
    * Buffers values into a linked list for specified time period. Oldest values 
    * are at the front of the buffer. Emits tuples of (element, time). 
    * Only emits items if `duration` has passed. When
    * new item arrives to the buffer, oldest one is removed.
    **/
    // TODO: TimeScale -> TimeContext & test
    public static IObservable<ReadOnlyLinkedList<Tpl<A, float>>> timeBuffer<A>(
      this IObservable<A> o, Duration duration, TimeScale timeScale = TimeScale.Realtime
    ) => o.timeBuffer(duration, new ObservableReadOnlyLinkedListQueue<Tpl<A, float>>(), timeScale);

    public static IObservable<C> timeBuffer<A, C>(
      this IObservable<A> o, Duration duration, IObservableQueue<Tpl<A, float>, C> queue,
      TimeScale timeScale = TimeScale.Realtime
    ) => Observable.a(ObservableOpImpls.timeBuffer(o, duration, queue, timeScale));

    #endregion

    /**
      * Joins events of two observables returning an observable which emits
      * events when either observable emits them.
      **/
    public static IObservable<A> join<A, B>(
      this IObservable<A> o, IObservable<B> other
    ) where B : A => Observable.a(ObservableOpImpls.join(o, other));

    #region #joinAll

    /**
     * Joins events of more than two observables effectively.
     **/
    public static IObservable<A> joinAll<A, B>(
      this IObservable<A> o, ICollection<IObservable<B>> others
    ) where B : A => o.joinAll(others, others.Count);

    public static IObservable<A> joinAll<A, B>(
      this IObservable<A> o, IEnumerable<IObservable<B>> others, int othersCount
    ) where B : A => Observable.a(ObservableOpImpls.joinAll(o, others, othersCount));

    #endregion

    /* Joins events, but discards the values. */
    public static IObservable<Unit> joinDiscard<A, X>(
      this IObservable<A> o, IObservable<X> other
    ) => Observable.a(ObservableOpImpls.joinDiscard(o, other));

    /** 
      * Only emits an event if other event was not emmited in specified 
      * time range.
      **/
    // TODO: test with integration tests
    public static IObservable<A> onceEvery<A>(
      this IObservable<A> o, Duration duration, TimeScale timeScale = TimeScale.Realtime
    ) => Observable.a(ObservableOpImpls.onceEvery(o, duration, timeScale));

    /**
      * Waits until `count` events are emmited within a single `timeframe` 
      * seconds window and emits a read only linked list of 
      * (element, emission time) Tpls with emmission time.
      **/
    public static IObservable<ReadOnlyLinkedList<Tpl<A, float>>> withinTimeframe<A>(
      this IObservable<A> o, int count, Duration timeframe, TimeScale timeScale = TimeScale.Realtime
    ) => ObservableOpImpls.withinTimeframe(o, count, timeframe, timeScale, Observable.a);

    /** Delays each event. **/
    public static IObservable<A> delayed<A>(
      this IObservable<A> o, Duration delay
    ) => Observable.a(ObservableOpImpls.delayed(o, delay));

    #region Changes

    // Returns pairs of (old, new) values when they are changing.
    // If there was no events before, old may be None.
    public static IObservable<Tpl<Option<A>, A>> changesOpt<A>(
      this IObservable<A> o, Fn<A, A, bool> areEqual = null
    ) => Observable.a(ObservableOpImpls.changesOpt(o, areEqual ?? EqComparer<A>.Default.Equals));

    // Like changesOpt() but does not emit if old was None.
    public static IObservable<Tpl<A, A>> changes<A>(
      this IObservable<A> o, Fn<A, A, bool> areEqual = null
    ) => Observable.a(ObservableOpImpls.changes(o, areEqual ?? EqComparer<A>.Default.Equals));

    // Emits new values. Always emits first value and then emits changed values.
    public static IObservable<A> changedValues<A>(
      this IObservable<A> o, Fn<A, A, bool> areEqual = null
    ) => Observable.a(ObservableOpImpls.changedValues(o, areEqual ?? EqComparer<A>.Default.Equals));

    #endregion

    // Convert this observable to reactive value with given initial value.
    public static IRxVal<A> toRxVal<A>(this IObservable<A> o, A initial) => 
      RxVal.a(initial, o.subscribe);

    #region #joinAll

    public static IObservable<A> joinAll<A>(
      this ICollection<IObservable<A>> observables
    ) => observables.joinAll(observables.Count);

    /**
     * Joins all events from all observables into one stream.
     **/
    public static IObservable<A> joinAll<A>(
      this IEnumerable<IObservable<A>> observables, int count
    ) =>
      Observable.a<A>(obs => ObservableOpImpls.multipleFinishes(obs, count, checkFinished => 
          observables.Select(aObs =>
            aObs.subscribe(obs.push, checkFinished)
          ).ToArray().joinSubscriptions()
        )
      );

    #endregion
  }
}