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
  /**
   * Notes:
   * 
   * #subscribe - if you subscribe to an observable during a callback, you 
   * will not get the current event.
   * 
   * <code>
   * void example(IObservable<A> observable) {
   *   observable.subscribe(a => {
   *     Log.info("A " + a);
   *     observable.subscribe(a1 => {
   *       Log.info("A1 " + a);
   *     });
   *   });
   * }
   * </code>
   * 
   * You will not get the A1 log statement here if you only submit one value into the observable.
   * TODO: write test
   * 
   * #submit - all subscribers will be notified about current value, before 
   * doing a submission of next value. Thus
   * 
   * <code>
   * void example(Subject<int> observable) {
   *   observable.subscribe(a => {
   *     Log.info("A1=" + a);
   *     if (a == 0) observable.push(a + 1);
   *   });
   *   observable.subscribe(a => {
   *     Log.info("A2=" + a);
   *   });
   *   observable.push(0);
   * }
   * </code>
   * 
   * Will print A1=0,A2=0 and then A1=1,A2=1, not A1=0,A1=1,A2=1,A2=0
   * TODO: write test
   **/
  public interface IObservable<A> {
    int subscribers { get; }
    ISubscription subscribe(Act<A> onChange);
    ISubscription subscribe(Act<A> onChange, Act onFinish);
    ISubscription subscribe(IObserver<A> observer);
    /** Emits first value to the future and unsubscribes. **/
    Future<A> toFuture();
    /* Pipes values (but not finishes) to given observer. */
    [Obsolete("Use #subscribe(observer.push) instead")]
    ISubscription pipeTo(IObserver<A> observer);
    /** Maps events coming from this observable. **/
    IObservable<B> map<B>(Fn<A, B> mapper);
    /**
     * Discards values that this observable emits, turning it into event 
     * source that does not carry data with it.
     **/
    IObservable<Unit> discardValue();
    /** 
     * Maps events coming from this observable and emits all events contained 
     * in returned enumerable.
     **/
    IObservable<B> flatMap<B>(Fn<A, IEnumerable<B>> mapper);
    /** 
     * Maps events coming from this observable and emits all events contained 
     * in returned observable.
     **/
    IObservable<B> flatMap<B>(Fn<A, IObservable<B>> mapper);
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
    // Emits new values. Always emits first value and then emits changed values.
    IObservable<A> changedValues();
    // Skips `count` values from the stream.
    IObservable<A> skip(uint count);
  }

  public interface IObserver<in A> {
    void push(A value);
    void finish();
  }

  public class Observer<A> : IObserver<A> {
    readonly Act<A> onValuePush;
    readonly Act onFinish;

    public Observer(Act<A> onValuePush, Act onFinish) {
      this.onValuePush = onValuePush;
      this.onFinish = onFinish;
    }

    public Observer(Act<A> onValuePush) {
      this.onValuePush = onValuePush;
      onFinish = () => {};
    }

    public void push(A value) { onValuePush(value); }
    public void finish() { onFinish(); }
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

    public static IObservable<A> empty<A>() { return Observable<A>.empty; }

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
          return new Subscription(cr.stop);
        })
      );
    } }

    public static IObservable<DateTime> interval(float intervalS, float delayS) 
    { return interval(intervalS, F.some(delayS)); }

    public static IObservable<DateTime> interval(
      float intervalS, Option<float> delayS=new Option<float>()
    ) {
      return new Observable<DateTime>(observer => {
        var cr = ASync.StartCoroutine(interval(observer, intervalS, delayS));
        return new Subscription(cr.stop);
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

  public class ObservableFinishedException : Exception {
    public ObservableFinishedException(string message) : base(message) {}
  }

  public class Observable<A> : IObservable<A> {
    public static readonly Observable<A> empty = 
      new Observable<A>(_ => new Subscription(() => {}));

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
      return subscribeFn => new Observable<Elem>(subscribeFn);
    }

    struct Sub {
      public readonly Subscription subscription;
      public bool active;
      public readonly IObserver<A> observer;

      public Sub(Subscription subscription, bool active, IObserver<A> observer) {
        this.subscription = subscription;
        this.active = active;
        this.observer = observer;
      }

      public override string ToString() { return string.Format(
        "Sub[subscription: {0}, active: {1}, observer: {2}]",
        subscription, active, observer
      ); }
    }

    // We need to preserve the order of subscriptions here.
    private SList8<Sub> subscriptions = new SList8<Sub>();
    private SList8<A> pendingSubmits = new SList8<A>();

    // Are we currently iterating through subscriptions?
    bool iterating;
    // We were iterating when #finish was called, so we have to finish when we clean up.
    bool willFinish;
    // Is this observable finished and will not take any more submits.
    bool finished;
    // How many subscription removals we have pending?
    private int pendingRemovals;

    private readonly Option<SourceProperties> sourceProps;

    protected Observable() {
      sourceProps = F.none<SourceProperties>();
    }

    public Observable(Fn<IObserver<A>, ISubscription> subscribeFn) {
      sourceProps = F.some(new SourceProperties(
        new Observer<A>(submit, finish), subscribeFn
      ));
    }

    protected void submit(A value) {
      if (finished) throw new ObservableFinishedException(string.Format(
        "Observable {0} is finished, but #submit called with {1}", this, value
      ));

      if (iterating) {
        // Do not submit if iterating.
        pendingSubmits.add(value);
        return;
      }

      // Mark a flag to prevent concurrent modification of subscriptions array.
      iterating = true;
      try {
        Profiler.BeginSample("submit loop");
        for (var idx = 0; idx < subscriptions.size; idx++) {
          var sub = subscriptions[idx];
          if (sub.active && sub.subscription.isSubscribed) {
            Profiler.BeginSample("loop step " + idx);
            sub.observer.push(value);
            Profiler.EndSample();
          }
        }
        Profiler.EndSample();
      }
      finally {
        iterating = false;
        afterIteration();
        // Process pending submits.
        if (pendingSubmits.size > 0) submit(pendingSubmits.removeAt(0));
      }
    }

    protected void finish() {
      if (iterating) {
        willFinish = true;
        return;
      }

      finished = true;
      iterating = true;
      for (var idx = 0; idx < subscriptions.size; idx++) {
        var sub = subscriptions[idx];
        sub.observer.finish();
        sub.subscription.unsubscribe();
      }
      iterating = false;
      afterIteration();
      subscriptions.clear();
    }

    public int subscribers 
    { get { return subscriptions.size - pendingRemovals; } }

    public ISubscription subscribe(Act<A> onChange) 
    { return subscribe(onChange, () => {}); }

    public ISubscription subscribe(Act<A> onChange, Act onFinish) 
    { return subscribe(new Observer<A>(onChange, onFinish)); }

    public virtual ISubscription subscribe(IObserver<A> observer) {
      var subscription = new Subscription(onUnsubscribed);
      subscriptions.add(new Sub(subscription, !iterating, observer));
      // Subscribe to source if we have a first subscriber.
      sourceProps.each(_ => _.trySubscribe());
      return subscription;
    }

    public Future<A> toFuture() {
      var f = new FutureImpl<A>();
      var subscription = subscribe(f.completeSuccess);
      f.onComplete(_ => subscription.unsubscribe());
      return f;
    }

    public ISubscription pipeTo(IObserver<A> observer) 
    { return subscribe(observer.push); }

    public IObservable<B> map<B>(Fn<A, B> mapper) {
      return mapImpl(mapper, builder<B>());
    }

    protected O mapImpl<B, O>(Fn<A, B> mapper, ObserverBuilder<B, O> builder) {
      return builder(obs => subscribe(val => obs.push(mapper(val))));
    }

    public IObservable<Unit> discardValue() 
    { return discardValueImpl(builder<Unit>()); }

    protected O discardValueImpl<O>(ObserverBuilder<Unit, O> builder) 
    { return mapImpl(_ => F.unit, builder); }

    public IObservable<B> flatMap<B>(Fn<A, IEnumerable<B>> mapper) {
      return flatMapImpl(mapper, builder<B>());
    }

    public O flatMapImpl<B, O>
    (Fn<A, IEnumerable<B>> mapper, ObserverBuilder<B, O> builder) {
      return builder(obs => subscribe(val => {
        foreach (var b in mapper(val)) obs.push(b);
      }));
    }

    public IObservable<B> flatMap<B>(Fn<A, IObservable<B>> mapper)
    { return flatMapImpl(mapper, builder<B>()); }

    public O flatMapImpl<B, O>
    (Fn<A, IObservable<B>> mapper, ObserverBuilder<B, O> builder) {
      return builder(obs => {
        ISubscription innerSub = null;
        Act innerUnsub = () => { if (innerSub != null) innerSub.unsubscribe(); };
        var thisSub = subscribe(val => {
          innerUnsub();
          innerSub = mapper(val).subscribe(obs.push);
        });
        return thisSub.join(new Subscription(innerUnsub));
      });
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

    protected O zipImpl<B, O>
    (IObservable<B> other, ObserverBuilder<Tpl<A, B>, O> builder) {
      return builder(obs => {
        var lastSelf = F.none<A>();
        var lastOther = F.none<B>();
        Action notify = () => lastSelf.each(aVal => lastOther.each(bVal =>
          obs.push(F.t(aVal, bVal))
        ));
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = other.subscribe(val => { lastOther = F.some(val); notify(); });
        return s1.join(s2);
      });
    }

    public IObservable<Tpl<A, B, C>> zip<B, C>(IObservable<B> o1, IObservable<C> o2)
    { return zipImpl(o1, o2, builder<Tpl<A, B, C>>()); }

    protected O zipImpl<B, C, O>
    (IObservable<B> o1, IObservable<C> o2, ObserverBuilder<Tpl<A, B, C>, O> builder) {
      return builder(obs => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        Action notify = () =>
          lastSelf.each(aVal =>
          lastO1.each(bVal =>
          lastO2.each(cVal =>
            obs.push(F.t(aVal, bVal, cVal))
          )));
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); });
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); });
        return s1.join(s2, s3);
      });
    }

    protected O zipImpl<B, C, D, O>
    (IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, ObserverBuilder<Tpl<A, B, C, D>, O> builder) {
      return builder(obs => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        var lastO3 = F.none<D>();
        Action notify = () => 
          lastSelf.each(aVal => 
          lastO1.each(bVal =>
          lastO2.each(cVal =>
          lastO3.each(dVal =>
            obs.push(F.t(aVal, bVal, cVal, dVal))
          ))));
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); });
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); });
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); });
        return s1.join(s2, s3, s4);
      });
    }

    public IObservable<Tpl<A, B, C, D>> zip<B, C, D>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3
    ) { return zipImpl(o1, o2, o3, builder<Tpl<A, B, C, D>>()); }

    protected O zipImpl<B, C, D, E, O>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4, 
      ObserverBuilder<Tpl<A, B, C, D, E>, O> builder
    ) {
      return builder(obs => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        var lastO3 = F.none<D>();
        var lastO4 = F.none<E>();
        Action notify = () => 
          lastSelf.each(aVal => 
          lastO1.each(bVal =>
          lastO2.each(cVal =>
          lastO3.each(dVal =>
          lastO4.each(eVal =>
            obs.push(F.t(aVal, bVal, cVal, dVal, eVal))
          )))));
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); });
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); });
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); });
        var s5 = o4.subscribe(val => { lastO4 = F.some(val); notify(); });
        return s1.join(s2, s3, s4, s5);
      });
    }

    public IObservable<Tpl<A, B, C, D, E>> zip<B, C, D, E>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4
    ) { return zipImpl(o1, o2, o3, o4, builder<Tpl<A, B, C, D, E>>()); }

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
        }, obs.finish);
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

    public IObservable<A> skip(uint count) { return skipImpl(builder<A>(), count); }

    protected O skipImpl<O>(ObserverBuilder<A, O> builder, uint count) {
      return builder(obs => {
        var skipped = 0u;
        return subscribe(
          value => {
            if (skipped >= count) obs.push(value);
            else skipped++;
          },
          obs.finish
        );
      });
    }

    private void onUnsubscribed() {
      pendingRemovals++;
      if (iterating) return;
      afterIteration();

      // Unsubscribe from source if we don't have any subscribers that are
      // subscribed to us.
      if (subscribers == 0) sourceProps.each(_ => _.tryUnsubscribe());
    }

    private void afterIteration() {
      if (pendingRemovals != 0) {
        for (var idx = 0; idx < subscriptions.size;) {
          var sub = subscriptions[idx];
#if DEBUG
          if (sub.subscription == null) throw new IllegalStateException(
            "sub="+sub+
            "\nidx="+idx+
            "\nsubscriptions="+subscriptions.asString()
          );
#endif
          if (!sub.subscription.isSubscribed) subscriptions.removeAt(idx);
          else idx++;
        }
        pendingRemovals = 0;
      }
      if (willFinish) {
        willFinish = false;
        finish();
      }
    }
  }
}