using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface IObservable<A> {
    int subscribers { get; }
    ISubscription subscribe(Act<A> onChange);
    ISubscription subscribe(Act<A, ISubscription> onChange);
    /** Emits first value to the future and unsubscribes. **/
    Future<A> toFuture();
    /** Maps events coming from this observable. **/
    IObservable<B> map<B>(Fn<A, B> mapper);
    /** 
     * Maps events coming from this observable and emits all events contained 
     * in returned enumerable.
     **/
    IObservable<B> flatMap<B>(Fn<A, IEnumerable<B>> mapper);
    /** Only emits events that pass the predicate. **/
    IObservable<A> filter(Fn<A, bool> predicate);
    /** Only emits events that return some. **/
    IObservable<B> collect<B>(Fn<A, Option<B>> collector);
    /**
     * Buffers values into a linked list of specified size. Oldest values 
     * are at the front of the buffer. Only emits `size` items at a time. When
     * new item arrives to the buffer, oldest one is removed.
     **/
    IObservable<ReadOnlyLinkedList<A>> buffer(int size);
    /**
     * Buffers values into a linked list for specified time period. Oldest values 
     * are at the front of the buffer. Emits tuples of (element, time), where time
     * is `Time.time`. Only emits items if `seconds` has passed. When
     * new item arrives to the buffer, oldest one is removed.
     **/
    IObservable<ReadOnlyLinkedList<Tpl<A, float>>> timeBuffer(float seconds);
    /**
     * Joins events of two observables returning an observable which emits
     * events when either observable emits them.
     **/
    IObservable<A> join<B>(IObservable<B> other) where B : A;
    /** 
     * Only emits an event if other event was not emmited in specified 
     * time range.
     **/
    IObservable<A> onceEvery(float seconds);
    /**
     * Waits until `count` events are emmited within a single `timeframe` 
     * seconds window and emits a read only linked list of 
     * (element, emmision time) Tpls with emmission time taken from 
     * `Time.time`.
     **/
    IObservable<ReadOnlyLinkedList<Tpl<A, float>>> withinTimeframe(int count, float timeframe);
    /** Delays each event X seconds. **/
    IObservable<A> delayed(float seconds);
    IObservable<Tpl<A, B>> zip<B>(IObservable<B> other);
    IObservable<Tpl<A, B, C>> zip<B, C>(IObservable<B> o1, IObservable<C> o2);
    IObservable<Tpl<A, B, C, D>> zip<B, C, D>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3
    );
    IObservable<Tpl<A, B, C, D, E>> zip<B, C, D, E>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4
    );
    // Returns pairs of (old, new) values when they are changing.
    // If there was no events before, old may be None.
    IObservable<Tpl<Option<A>, A>> changesOpt();
    // Like changesOpt() but does not emit if old was None.
    IObservable<Tpl<A, A>> changes();
    // Like changes() but only emits new values.
    IObservable<A> changedValues();
  }

  public interface IObserver<in A> {
    void push(A value);
  }

  public class Observer<A> : IObserver<A> {
    private readonly Act<A> onValuePush;

    public Observer(Act<A> onValuePush) {
      this.onValuePush = onValuePush;
    }

    public void push(A value) {
      onValuePush(value);
    }
  }

  public static class Observable {
    public static Tpl<A, IObservable<Evt>> a<A, Evt>
    (Fn<IObserver<Evt>, Tpl<A, ISubscription>> creator) {
      IObserver<Evt> observer = null;
      ISubscription subscription = null;
      var observable = new Observable<Evt>(obs => {
        observer = obs;
        return subscription;
      });
      var t = creator(observer);
      var obj = t._1;
      subscription = t._2;
      return F.t(obj, (IObservable<Evt>) observable);
    }

    public static IObservable<A> fromEvent<A>(
      Act<Act<A>> registerCallback, Act unregisterCallback
    ) {
      return new Observable<A>(obs => {
        registerCallback(obs.push);
        return new Subscription(unregisterCallback);
      });
    }

    private static IObservable<Unit> everyFrameInstance;

    public static IObservable<Unit> everyFrame { get {
      return everyFrameInstance ?? (
        everyFrameInstance = new Observable<Unit>(observer => {
          var cr = ASync.StartCoroutine(everyFrameCR(observer));
          return new Subscription(() => ASync.StopCoroutine(cr));
        })
      );
    } }

    public static IObservable<DateTime> interval(float intervalS, float delayS) 
    { return interval(intervalS, F.some(delayS)); }

    public static IObservable<DateTime> interval(
      float intervalS, Option<float> delayS=
#if UNITY_IOS
      null
#else
      new Option<float>()
#endif
    ) {
#if UNITY_IOS
      if (delayS == null) delayS = new Option<float>();
#endif
      return new Observable<DateTime>(observer => {
        var cr = ASync.StartCoroutine(interval(observer, intervalS, delayS));
        return new Subscription(() => ASync.StopCoroutine(cr));
      });
    }

    public static IObservable<Tpl<P1, P2, P3, P4>> Tpl<P1, P2, P3, P4>(
      IObservable<P1> o1, IObservable<P2> o2, IObservable<P3> o3, IObservable<P4> o4
    ) {
      return o1.zip<P2>(o2).zip<P3>(o3).zip<P4>(o4).
        map<Tpl<P1, P2, P3, P4>>(t => F.t(t._1._1._1, t._1._1._2, t._1._2, t._2));
    }

    private static IEnumerator everyFrameCR(IObserver<Unit> observer) {
      while (true) {
        observer.push(Unit.instance);
        yield return null;
      }
    }

    private static IEnumerator interval(
      IObserver<DateTime> observer, float intervalS, Option<float> delayS
    ) {
      if (delayS.isDefined) yield return new WaitForSeconds(delayS.get);
      var wait = new WaitForSeconds(intervalS);
      while (true) {
        observer.push(DateTime.Now);
        yield return wait;
      }
    }
  }

  public delegate ObservableImplementation ObserverBuilder<
    in Elem, out ObservableImplementation
  >(Fn<IObserver<Elem>, ISubscription> subscriptionFn);

  public class Observable<A> : IObservable<A> {
    /** Properties if this observable was created from other source. **/
    private class SourceProperties {
      private readonly IObserver<A> observer;
      private readonly Fn<IObserver<A>, ISubscription> subscribeFn;

      private Option<ISubscription> subscription = F.none<ISubscription>();

      public SourceProperties(
        IObserver<A> observer, Fn<IObserver<A>, ISubscription> subscribeFn
      ) {
        this.observer = observer;
        this.subscribeFn = subscribeFn;
      }

      public bool trySubscribe() {
        return subscription.fold(
          () => {
            subscription = F.some(subscribeFn(observer));
            return true;
          },
          _ => false
        );
      }

      public bool tryUnsubscribe() {
        return subscription.fold(
          () => false, 
          s => {
            subscription = F.none<ISubscription>();
            return s.unsubscribe();
          }
        );
      }
    }

    private static ObserverBuilder<Elem, IObservable<Elem>> builder<Elem>() {
      return builder => new Observable<Elem>(builder);
    }

    private readonly RandomList<Tpl<Subscription, Act<A>>> subscriptions =
      new RandomList<Tpl<Subscription, Act<A>>>();

    // Are we currently iterating through subscriptions?
    private bool iterating;
    // How many subscription removals we have pending?
    private int pendingRemovals;

    private readonly Option<SourceProperties> sourceProps;

    protected Observable() {
      sourceProps = F.none<SourceProperties>();
    }

    public Observable(Fn<IObserver<A>, ISubscription> subscribeFn) {
      sourceProps = F.some(new SourceProperties(
        new Observer<A>(submit), subscribeFn
      ));
    }

    protected void submit(A value) {
      // Mark a flag to prevent concurrent modification of subscriptions array.
      iterating = true;
      try {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < subscriptions.Count; idx++) {
          var t = subscriptions[idx];
          var subscription = t._1;
          var act = t._2;
          if (subscription.isSubscribed) act(value);
        }
      }
      finally {
        iterating = false;
        cleanupSubscriptions();
      }
    }

    public int subscribers { get { return subscriptions.Count - pendingRemovals; } }

    public virtual ISubscription subscribe(Act<A> onChange) {
      var subscription = new Subscription(onUnsubscribed);
      // We can safely add to the subscriptions lists end.
      subscriptions.Add(F.t(subscription, onChange));
      // Subscribe to source if we have a first subscriber.
      sourceProps.each(_ => _.trySubscribe());
      return subscription;
    }

    public ISubscription subscribe(Act<A, ISubscription> onChange) {
      ISubscription subscription = null;
      // ReSharper disable once AccessToModifiedClosure
      subscription = subscribe(a => onChange(a, subscription));
      return subscription;
    }

    public Future<A> toFuture() {
      var f = new FutureImpl<A>();
      var subscription = subscribe(f.completeSuccess);
      f.onComplete(_ => subscription.unsubscribe());
      return f;
    }

    public IObservable<B> map<B>(Fn<A, B> mapper) {
      return mapImpl(mapper, builder<B>());
    }

    protected O mapImpl<B, O>(Fn<A, B> mapper, ObserverBuilder<B, O> builder) {
      return builder(obs => subscribe(val => obs.push(mapper(val))));
    }

    public IObservable<B> flatMap<B>(Fn<A, IEnumerable<B>> mapper) {
      return flatMapImpl(mapper, builder<B>());
    }

    public O flatMapImpl<B, O>
    (Fn<A, IEnumerable<B>> mapper, ObserverBuilder<B, O> builder) {
      return builder(obs => subscribe(val => {
        foreach (var b in mapper(val)) obs.push(b);
      }));
    }

    public IObservable<A> filter(Fn<A, bool> predicate) {
      return filterImpl(predicate, builder<A>());
    }

    protected O filterImpl<O>
    (Fn<A, bool> predicate, ObserverBuilder<A, O> builder) {
      return builder(obs => subscribe(val => {
        if (predicate(val)) obs.push(val);
      }));
    }

    public IObservable<B> collect<B>(Fn<A, Option<B>> collector) {
      return collectImpl(collector, builder<B>());
    }

    protected O collectImpl<O, B>
    (Fn<A, Option<B>> collector, ObserverBuilder<B, O> builder) {
      return builder(obs => subscribe(val => collector(val).each(obs.push)));
    }

    public IObservable<ReadOnlyLinkedList<A>> buffer(int size) {
      return bufferImpl(size, builder<ReadOnlyLinkedList<A>>());
    }

    protected O bufferImpl<O>
    (int size, ObserverBuilder<ReadOnlyLinkedList<A>, O> builder) {
      return builder(obs => {
        var buffer = new LinkedList<A>();
        var roFacade = new ReadOnlyLinkedList<A>(buffer);
        return subscribe(val => {
          buffer.AddLast(val);
          if (buffer.Count > size) buffer.RemoveFirst();
          obs.push(roFacade);
        });
      });
    }

    public IObservable<ReadOnlyLinkedList<Tpl<A, float>>> timeBuffer(float seconds) {
      return timeBufferImpl(seconds, builder<ReadOnlyLinkedList<Tpl<A, float>>>());
    }

    protected O timeBufferImpl<O>
    (float seconds, ObserverBuilder<ReadOnlyLinkedList<Tpl<A, float>>, O> builder) {
      return builder(obs => {
        var buffer = new LinkedList<Tpl<A, float>>();
        var roFacade = ReadOnlyLinkedList.a(buffer);
        return subscribe(val => {
          buffer.AddLast(F.t(val, Time.time));
          var lastTime = buffer.Last.Value._2;
          if (buffer.First.Value._2 + seconds <= lastTime) {
            // Remove items which are too old.
            while (buffer.First.Value._2 + seconds < lastTime) 
              buffer.RemoveFirst(); 
            obs.push(roFacade);
          }
        });
      });
    }

    public IObservable<A> join<B>(IObservable<B> other) where B : A {
      return joinImpl(other, builder<A>());
    }

    protected O joinImpl<B, O>
    (IObservable<B> other, ObserverBuilder<A, O> builder) where B : A {
      return builder(obs =>
        subscribe(obs.push).join(other.subscribe(v => obs.push(v)))
      );
    }

    public IObservable<A> onceEvery(float seconds) {
      return onceEveryImpl(seconds, builder<A>());
    }

    protected O onceEveryImpl<O>
    (float seconds, ObserverBuilder<A, O> builder) {
      return builder(obs => {
        var lastEmit = float.NegativeInfinity;
        return subscribe(value => {
          if (lastEmit + seconds > Time.time) return;
          lastEmit = Time.time;
          obs.push(value);
        });
      });
    }

    public IObservable<ReadOnlyLinkedList<Tpl<A, float>>> 
    withinTimeframe(int count, float timeframe) {
      return withinTimeframeImpl(
        count, timeframe, builder<ReadOnlyLinkedList<Tpl<A, float>>>()
      );
    }

    protected O withinTimeframeImpl<O>(
      int count, float timeframe, 
      ObserverBuilder<ReadOnlyLinkedList<Tpl<A, float>>, O> builder
    ) {
      return builder(obs => 
        map(value => F.t(value, Time.time)).
        buffer(count).
        filter(events => {
          if (events.Count != count) return false;
          var last = events.Last.Value._2;

          return events.All(t => last - t._2 <= timeframe);
        }).subscribe(obs.push)
      );
    }

    public IObservable<A> delayed(float seconds) {
      return delayedImpl(seconds, builder<A>());
    }

    protected O delayedImpl<O>(
      float seconds, ObserverBuilder<A, O> builder
    ) {
      return builder(obs => 
        subscribe(v => ASync.WithDelay(seconds, () => obs.push(v)))
      );
    }

    public IObservable<Tpl<A, B>> zip<B>(IObservable<B> other) {
      return zipImpl(other, builder<Tpl<A, B>>());
    }

    public IObservable<Tpl<A, B, C>> zip<B, C>(IObservable<B> o1, IObservable<C> o2) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug.
      return zip<B>(o1).zip<C>(o2).
        map<Tpl<A, B, C>>(t => F.t(t._1._1, t._1._2, t._2));
    }

    public IObservable<Tpl<A, B, C, D>> zip<B, C, D>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3
    ) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug.
      return zip<B, C>(o1, o2).zip<D>(o3).
        map<Tpl<A, B, C, D>>(t => F.t(t._1._1, t._1._2, t._1._3, t._2));
    }

    public IObservable<Tpl<A, B, C, D, E>> zip<B, C, D, E>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4
    ) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug.
      return zip<B, C, D>(o1, o2, o3).zip<E>(o4).map<Tpl<A, B, C, D, E>>(t => 
        F.t(t._1._1, t._1._2, t._1._3, t._1._4, t._2)
      );
    }

    protected O zipImpl<B, O>
    (IObservable<B> other, ObserverBuilder<Tpl<A, B>, O> builder) {
      return builder(obs => {
        var lastSelf = F.none<A>();
        var lastOther = F.none<B>();
        Action notify = () => lastSelf.each(aVal => lastOther.each(bVal =>
          obs.push(F.t(aVal, bVal))
        ));
        var s1 = subscribe(val => {
          lastSelf = F.some(val);
          notify();
        });
        var s2 = other.subscribe(val => {
          lastOther = F.some(val);
          notify();
        });
        return s1.join(s2);
      });
    }

    public IObservable<Tpl<Option<A>, A>> changesOpt() {
      return changesOptImpl(builder<Tpl<Option<A>, A>>());
    }

    private O changesBase<Elem, O>(
      Act<IObserver<Elem>, Option<A>, A> action, ObserverBuilder<Elem, O> builder
    ) {
      return builder(obs => {
        var lastValue = F.none<A>();
        return subscribe(val => {
          action(obs, lastValue, val);
          lastValue = F.some(val);
        });
      });
    }

    protected O changesOptImpl<O>(ObserverBuilder<Tpl<Option<A>, A>, O> builder) {
      return changesBase((obs, lastValue, val) => {
        var valueChanged = lastValue.fold(
          () => true,
          lastVal => EqComparer<A>.Default.Equals(lastVal, val)
        );
        if (valueChanged) obs.push(F.t(lastValue, val));
      }, builder);
    }

    public IObservable<Tpl<A, A>> changes() {
      return changesImpl(builder<Tpl<A, A>>());
    }

    protected O changesImpl<O>(ObserverBuilder<Tpl<A, A>, O> builder) {
      return changesBase((obs, lastValue, val) => lastValue.each(lastVal => {
        if (! EqComparer<A>.Default.Equals(lastVal, val))
          obs.push(F.t(lastVal, val));
      }), builder);
    }

    public IObservable<A> changedValues() {
      return changedValuesImpl(builder<A>());
    }

    protected O changedValuesImpl<O>(ObserverBuilder<A, O> builder) {
      return changesBase((obs, lastValue, val) => {
        if (lastValue.isEmpty) obs.push(val);
        else if (! EqComparer<A>.Default.Equals(lastValue.get, val))
          obs.push(val);
      }, builder);
    }

    private void onUnsubscribed() {
      pendingRemovals++;
      if (iterating) return;
      cleanupSubscriptions();

      // Unsubscribe from source if we don't have any subscribers that are
      // subscribed to us.
      if (subscribers == 0) sourceProps.each(_ => _.tryUnsubscribe());
    }

    private void cleanupSubscriptions() {
      if (pendingRemovals == 0) return;

      subscriptions.RemoveWhere(t => !t._1.isSubscribed);
      pendingRemovals = 0;
    }
  }
}