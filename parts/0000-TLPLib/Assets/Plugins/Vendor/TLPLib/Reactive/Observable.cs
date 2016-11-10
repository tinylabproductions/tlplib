using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
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
   **/
  public interface IObservable<A> {
    int subscribers { get; }
    bool finished { get; }
    ISubscription subscribe(Act<A> onChange);
    ISubscription subscribe(Act<A, ISubscription> onChange);
    ISubscription subscribe(Act<A> onChange, Action onFinish);
    ISubscription subscribe(IObserver<A> observer);
    /** Emits first value to the future and unsubscribes. **/
    Future<A> toFuture();
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
     * Maps events coming from this observable and emits events from returned futures.
     * 
     * Does not emit value if future completes after observable is finished.
     **/
    IObservable<B> flatMap<B>(Fn<A, Future<B>> mapper);
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
    IObservable<C> buffer<C>(int size, IObservableQueue<A, C> queue);
    /**
     * Buffers values into a linked list for specified time period. Oldest values 
     * are at the front of the buffer. Emits tuples of (element, time). 
     * Only emits items if `duration` has passed. When
     * new item arrives to the buffer, oldest one is removed.
     **/
    IObservable<ReadOnlyLinkedList<Tpl<A, float>>> timeBuffer(
      Duration duration, TimeScale timeScale = TimeScale.Realtime
    );
    IObservable<C> timeBuffer<C>(
      Duration duration, IObservableQueue<Tpl<A, float>, C> queue, 
      TimeScale timeScale = TimeScale.Realtime
    );
    /**
     * Joins events of two observables returning an observable which emits
     * events when either observable emits them.
     **/
    IObservable<A> join<B>(IObservable<B> other) where B : A;
    /**
     * Joins events of more than two observables effectively.
     **/
    IObservable<A> joinAll<B>(ICollection<IObservable<B>> others) where B : A;
    IObservable<A> joinAll<B>(IEnumerable<IObservable<B>> others, int otherCount) where B : A;
    /* Joins events, but discards the values. */
    IObservable<Unit> joinDiscard<X>(IObservable<X> other);
    /** 
     * Only emits an event if other event was not emmited in specified 
     * time range.
     **/
    IObservable<A> onceEvery(Duration timeframe, TimeScale timeScale = TimeScale.Realtime);
    /**
     * Waits until `count` events are emmited within a single `timeframe` 
     * seconds window and emits a read only linked list of 
     * (element, emission time) Tpls with emmission time.
     **/
    IObservable<ReadOnlyLinkedList<Tpl<A, float>>> withinTimeframe(
      int count, Duration timeframe, TimeScale timeScale = TimeScale.Realtime
    );
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
    IObservable<Tpl<A, A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, IObservable<A4> o4,
      IObservable<A5> o5
    );
    // Returns pairs of (old, new) values when they are changing.
    // If there was no events before, old may be None.
    IObservable<Tpl<Option<A>, A>> changesOpt(Fn<A, A, bool> areEqual = null);
    // Like changesOpt() but does not emit if old was None.
    IObservable<Tpl<A, A>> changes(Fn<A, A, bool> areEqual = null);
    // Emits new values. Always emits first value and then emits changed values.
    IObservable<A> changedValues(Fn<A, A, bool> areEqual = null);
    // Skips `count` values from the stream.
    IObservable<A> skip(uint count);
    // Convert this observable to reactive value with given initial value.
    IRxVal<A> toRxVal(A initial);
    // If several events are emitted per same frame, only emit last one in late update.
    IObservable<A> oncePerFrame();
    /** Return self as IObservable. */
    IObservable<A> asObservable { get; }

    // Logs actions at verbose level to standard logger.
    IObservable<A> setLogging(bool value);
  }

  public interface IObserver<in A> {
    void push(A value);
    void finish();
  }

  public static class IObserverExts {
    public static void pushMany<A>(this IObserver<A> obs, params A[] items) {
      foreach (var a in items) obs.push(a);
    }
  }

  public class Observer<A> : IObserver<A> {
    readonly Act<A> onValuePush;
    readonly Action onFinish;

    public Observer(Act<A> onValuePush, Action onFinish) {
      this.onValuePush = onValuePush;
      this.onFinish = onFinish;
    }

    public Observer(Act<A> onValuePush) {
      this.onValuePush = onValuePush;
      onFinish = () => {};
    }

    public void push(A value) => onValuePush(value);
    public void finish() => onFinish();
  }

  public static class Observable {
    public static ISubscription subscribeForOneEvent<A>(
      this IObservable<A> observable, Act<A> onEvent
    ) => observable.subscribe((a, sub) => {
      sub.unsubscribe();
      onEvent(a);
    });

    public static IObservableQueue<A, C> createQueue<A, C>(
      Act<A> addLast, Action removeFirst,
      Fn<int> count, Fn<C> collection, Fn<A> first, Fn<A> last
    ) => new ObservableLambdaQueue<A, C>(
      addLast, removeFirst, count, collection, first, last
    );

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

    public static IObservable<A> empty<A>() => Observable<A>.empty;

    public static IObservable<A> fromEvent<A>(
      Act<Act<A>> registerCallback, Action unregisterCallback
    ) {
      return new Observable<A>(obs => {
        registerCallback(obs.push);
        return new Subscription(unregisterCallback);
      });
    }

    static IObservable<Unit> everyFrameInstance;

    public static IObservable<Unit> everyFrame =>
      everyFrameInstance ?? (
        everyFrameInstance = new Observable<Unit>(observer => {
          var cr = ASync.StartCoroutine(everyFrameCR(observer));
          return new Subscription(cr.stop);
        })
      );

    #region joinAll

    public static IObservable<A> joinAll<A>(
      this ICollection<IObservable<A>> observables
    ) => observables.joinAll(observables.Count);

    /**
     * Joins all events from all observables into one stream.
     **/
    public static IObservable<A> joinAll<A>(
      this IEnumerable<IObservable<A>> observables, int count
    ) =>
      new Observable<A>(obs => 
        Observable<A>.multipleFinishes(obs, count, checkFinished => 
          observables.Select(aObs =>
            aObs.subscribe(obs.push, checkFinished)
          ).ToArray().joinSubscriptions()
        )
      );

    #endregion

    #region touches

    public struct Touch {
      public readonly int fingerId;
      public readonly Vector2 position, previousPosition;
      public readonly int tapCount;
      public readonly TouchPhase phase;

      public Touch(int fingerId, Vector2 position, Vector2 previousPosition, int tapCount, TouchPhase phase) {
        this.fingerId = fingerId;
        this.position = position;
        this.previousPosition=previousPosition;
        this.tapCount = tapCount;
        this.phase = phase;
      }
    }

    static IObservable<List<Touch>> touchesInstance;

    public static IObservable<List<Touch>> touches => 
      touchesInstance ?? (touchesInstance = createTouchesInstance());

    static IObservable<List<Touch>> createTouchesInstance() {
      var touchList = new List<Touch>();
      var previousMousePos = new Vector2();
      var previousMousePhase = TouchPhase.Ended;
      var prevPositions = new Dictionary<int, Vector2>();
      return everyFrame.map(_ => {
        touchList.Clear();
        if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) {
          var curPos = (Vector2) Input.mousePosition;
          var curPhase = Input.GetMouseButtonDown(0)
            ? TouchPhase.Began
            : Input.GetMouseButtonUp(0)
              ? TouchPhase.Ended
              : curPos == previousMousePos ? TouchPhase.Moved : TouchPhase.Stationary;
          if (previousMousePhase == TouchPhase.Ended) previousMousePos = curPos;
          touchList.Add(new Touch(-100, curPos, previousMousePos, 0, curPhase));
          previousMousePos = curPos;
          previousMousePhase = curPhase;
        }
        for (var i = 0; i < Input.touchCount; i++) {
          var t = Input.GetTouch(i);
          var id = t.fingerId;
          var previousPos = t.position;
          if (t.phase != TouchPhase.Began) {
            if (!prevPositions.TryGetValue(id, out previousPos)) {
              previousPos = t.position;
            }
            prevPositions[id] = t.position;
          }
          if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended) {
            prevPositions.Remove(id);
          }
          touchList.Add(new Touch(t.fingerId, t.position, previousPos, t.tapCount, t.phase));
        }
        return touchList;
      });
    }

    #endregion

    public static IObservable<DateTime> interval(Duration interval, Duration delay) => 
      Observable.interval(interval, F.some(delay));

    public static IObservable<DateTime> interval(
      Duration interval, Option<Duration> delay=default(Option<Duration>)
    ) {
      Option.ensureValue(ref delay);
      return new Observable<DateTime>(observer => {
        var cr = ASync.StartCoroutine(intervalEnum(observer, interval, delay));
        return new Subscription(cr.stop);
      });
    }

    static IEnumerator everyFrameCR(IObserver<Unit> observer) {
      while (true) {
        observer.push(Unit.instance);
        yield return null;
      }
      // ReSharper disable once IteratorNeverReturns
    }

    static IEnumerator intervalEnum(
      IObserver<DateTime> observer, Duration interval, Option<Duration> delay
    ) {
      foreach (var d in delay) yield return new WaitForSeconds(d.seconds);
      var wait = new WaitForSeconds(interval.seconds);
      while (true) {
        observer.push(DateTime.Now);
        yield return wait;
      }
      // ReSharper disable once IteratorNeverReturns
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
      new Observable<A>(_ => new Subscription(() => { }));

    /** Properties if this observable was created from other source. **/
    class SourceProperties {
      readonly IObserver<A> observer;
      readonly Fn<IObserver<A>, ISubscription> subscribeFn;
      public readonly bool beAlwaysSubscribed;

      Option<ISubscription> subscription = F.none<ISubscription>();

      public SourceProperties(
        IObserver<A> observer, Fn<IObserver<A>, ISubscription> subscribeFn, 
        bool beAlwaysSubscribed
      ) {
        this.observer = observer;
        this.subscribeFn = subscribeFn;
        this.beAlwaysSubscribed = beAlwaysSubscribed;
      }

      public bool trySubscribe() => 
        subscription.fold(
          () => {
            subscription = F.some(subscribeFn(observer));
            return true;
          },
          _ => false
        );

      public bool tryUnsubscribe() => 
        subscription.fold(
          () => false, 
          s => {
            subscription = F.none<ISubscription>();
            return s.unsubscribe();
          }
        );
    }

    static ObserverBuilder<Elem, IObservable<Elem>> builder<Elem>() {
      return builder => new Observable<Elem>(builder);
    }

    struct Sub {
      public readonly Subscription subscription;
      public readonly IObserver<A> observer;

      public bool active;

      public Sub(Subscription subscription, IObserver<A> observer, bool active) {
        this.subscription = subscription;
        this.observer = observer;
        this.active = active;
      }

      public override string ToString() => 
        $"{nameof(Sub)}[" +
        $"{nameof(subscription)}: {subscription}, " +
        $"{nameof(observer)}: {observer}, " +
        $"{nameof(active)}: {active}" +
        $"]";
    }

    readonly RandomList<Sub> subscriptions = new RandomList<Sub>();
    SList4<A> pendingSubmits = new SList4<A>();

    // Are we currently iterating through subscriptions?
    protected bool iterating { get; private set; }
    // We were iterating when #finish was called, so we have to finish when we clean up.
    bool willFinish;
    // Is this observable finished and will not take any more submits.
    public bool finished { get; private set; }
    // How many subscription activations do we have pending?
    int pendingSubscriptionActivations;
    // How many subscription removals we have pending?
    int pendingRemovals;

    readonly Option<SourceProperties> sourceProps;

    bool doLogging;

    protected Observable() {
      sourceProps = F.none<SourceProperties>();
    }

    public Observable(
      Fn<IObserver<A>, ISubscription> subscribeFn,
      bool beAlwaysSubscribed = false
    ) {
      var sourceProps = new SourceProperties(
        new Observer<A>(submit, finishObservable), subscribeFn,
        beAlwaysSubscribed
      );
      if (sourceProps.beAlwaysSubscribed) subscribeToSource(sourceProps);
      this.sourceProps = F.some(sourceProps);
    }

    protected virtual void submit(A value) {
      if (doLogging) {
        if (Log.isVerbose) Log.verbose($"[{nameof(Observable<A>)}] submit: {value}");
      }

      if (finished) throw new ObservableFinishedException(
        $"Observable {this} is finished, but #submit called with {value}"
      );

      if (iterating) {
        // Do not submit if iterating.
        pendingSubmits.add(value);
        return;
      }

      // Mark a flag to prevent concurrent modification of subscriptions array.
      iterating = true;
      try {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < subscriptions.Count; idx++) {
          var sub = subscriptions[idx];
          if (sub.active && sub.subscription.isSubscribed) sub.observer.push(value);
        }
      }
      finally {
        iterating = false;
        afterIteration();
        // Process pending submits.
        if (pendingSubmits.size > 0) submit(pendingSubmits.removeAt(0));
      }
    }

    protected void finishObservable() {
      if (iterating) {
        willFinish = true;
        return;
      }

      finished = true;
      iterating = true;
      for (var idx = 0; idx < subscriptions.Count; idx++) {
        var sub = subscriptions[idx];
        sub.observer.finish();
        sub.subscription.unsubscribe();
      }
      iterating = false;
      afterIteration();
      subscriptions.Clear();
    }

    public int subscribers => subscriptions.Count - pendingSubscriptionActivations - pendingRemovals;

    public ISubscription subscribe(Act<A> onChange) => subscribe(onChange, () => {});

    public ISubscription subscribe(Act<A> onChange, Action onFinish) => 
      subscribe(new Observer<A>(onChange, onFinish));

    public virtual ISubscription subscribe(IObserver<A> observer) {
      if (doLogging && Log.isVerbose)
        Log.verbose($"[{nameof(Observable<A>)}] subscribe: {observer}");
      var subscription = new Subscription(onUnsubscribed);
      var active = !iterating;
      subscriptions.Add(new Sub(subscription, observer, active));
      if (!active) pendingSubscriptionActivations++;
      
      // Subscribe to source if we have a first subscriber.
      foreach (var source in sourceProps) subscribeToSource(source);
      return subscription;
    }

    public ISubscription subscribe(Act<A, ISubscription> onChange) {
      ISubscription subscription = null;
      // ReSharper disable once AccessToModifiedClosure
      subscription = subscribe(a => onChange(a, subscription));
      return subscription;
    }

    public Future<A> toFuture() => 
      Future<A>.async((p, f) => {
        var subscription = subscribe(p.complete);
        f.onComplete(_ => subscription.unsubscribe());
      });

    #region #map

    public IObservable<B> map<B>(Fn<A, B> mapper) => 
      mapImpl(mapper, builder<B>());

    protected O mapImpl<B, O>(Fn<A, B> mapper, ObserverBuilder<B, O> builder) => 
      builder(obs => subscribe(val => obs.push(mapper(val)), obs.finish));

    #endregion

    #region #discardValue

    public IObservable<Unit> discardValue() =>
      discardValueImpl(builder<Unit>());

    protected O discardValueImpl<O>(ObserverBuilder<Unit, O> builder) =>
      builder(obs => subscribe(_ => obs.push(F.unit), obs.finish));

    #endregion

    #region #flatMap

    public IObservable<B> flatMap<B>(Fn<A, IEnumerable<B>> mapper) => 
      flatMapImpl(mapper, builder<B>());

    protected O flatMapImpl<B, O>
    (Fn<A, IEnumerable<B>> mapper, ObserverBuilder<B, O> builder) => 
      builder(obs => subscribe(val => {
        foreach (var b in mapper(val)) obs.push(b);
      }, obs.finish));

    public IObservable<B> flatMap<B>(Fn<A, IObservable<B>> mapper) => 
      flatMapImpl(mapper, builder<B>());

    protected O flatMapImpl<B, O>
    (Fn<A, IObservable<B>> mapper, ObserverBuilder<B, O> builder) => 
      builder(obs => {
        ISubscription innerSub = null;
        Action innerUnsub = () => innerSub?.unsubscribe();
        var thisSub = subscribe(
          val => {
            innerUnsub();
            innerSub = mapper(val).subscribe(obs);
          },
          () => {
            innerUnsub();
            obs.finish();
          }
        );
        return thisSub.join(new Subscription(innerUnsub));
      });

    public IObservable<B> flatMap<B>(Fn<A, Future<B>> mapper) => 
      flatMapImpl(mapper, builder<B>());

    protected O flatMapImpl<B, O>
    (Fn<A, Future<B>> mapper, ObserverBuilder<B, O> builder) => 
      builder(obs => {
        var sourceFinished = false;
        return subscribe(
          a => mapper(a).onComplete(b => {
            if (!sourceFinished) obs.push(b);
          }),
          () => {
            sourceFinished = true;
            obs.finish();
          }
        );
      });

    #endregion

    #region #filter

    public IObservable<A> filter(Fn<A, bool> predicate) => 
      filterImpl(predicate, builder<A>());

    protected O filterImpl<O>
    (Fn<A, bool> predicate, ObserverBuilder<A, O> builder) => 
      builder(obs => subscribe(
        val => { if (predicate(val)) obs.push(val); },
        obs.finish
      ));

    #endregion

    #region #collect

    public IObservable<B> collect<B>(Fn<A, Option<B>> collector) => 
      collectImpl(collector, builder<B>());

    protected O collectImpl<O, B>
    (Fn<A, Option<B>> collector, ObserverBuilder<B, O> builder) => 
      builder(obs => subscribe(
        val => { foreach (var b in collector(val)) obs.push(b); },
        obs.finish
      ));

    #endregion

    #region #buffer

    public IObservable<ReadOnlyLinkedList<A>> buffer(int size) => 
      buffer(size, new ObservableReadOnlyLinkedListQueue<A>());

    public IObservable<C> buffer<C>(int size, IObservableQueue<A, C> queue) =>
      bufferImpl(size, queue, builder<C>());

    protected O bufferImpl<O, C>(
      int size, IObservableQueue<A, C> queue,
      ObserverBuilder<C, O> builder
    ) => 
      builder(obs => subscribe(
        val => {
          queue.addLast(val);
          if (queue.count > size) queue.removeFirst();
          obs.push(queue.collection);
        },
        obs.finish
      ));

    #endregion

    #region #timeBuffer

    // TODO: test with integration tests
    public IObservable<ReadOnlyLinkedList<Tpl<A, float>>> timeBuffer(
      Duration duration, TimeScale timeScale
    ) => timeBuffer(duration, new ObservableReadOnlyLinkedListQueue<Tpl<A, float>>(), timeScale);

    public IObservable<C> timeBuffer<C>(
      Duration duration, IObservableQueue<Tpl<A, float>, C> queue,
      TimeScale timeScale
    ) => timeBufferImpl(duration, queue, timeScale, builder<C>());

    protected O timeBufferImpl<O, C>(
      Duration duration, 
      IObservableQueue<Tpl<A, float>, C> queue,
      TimeScale timeScale, 
      ObserverBuilder<C, O> builder
    ) => 
      builder(obs => subscribe(val => {
        queue.addLast(F.t(val, Time.time));
        var lastTime = queue.last._2;
        if (queue.first._2 + duration.seconds <= lastTime) {
          // Remove items which are too old.
          while (queue.first._2 + duration.seconds < lastTime) 
            queue.removeFirst(); 
          obs.push(queue.collection);
        }
      }));

    #endregion

    #region #join

    public IObservable<A> join<B>(IObservable<B> other) where B : A => 
      joinImpl(other, builder<A>());

    protected O joinImpl<B, O>
    (IObservable<B> other, ObserverBuilder<A, O> builder) where B : A => 
      builder(obs => multipleFinishes(obs, 2, checkFinished =>
        subscribe(
          obs.push,
          checkFinished
        ).join(other.subscribe(
          v => obs.push(v),
          checkFinished
        ))
      ));

    #endregion

    #region #joinAll

    public IObservable<A> joinAll<B>(ICollection<IObservable<B>> others) where B : A =>
      joinAll(others, others.Count);

    public IObservable<A> joinAll<B>(
      IEnumerable<IObservable<B>> others, int othersCount
    ) where B : A =>
      joinAllImpl(others, othersCount, builder<A>());

    protected O joinAllImpl<B, O>(
      IEnumerable<IObservable<B>> others, int othersCount, ObserverBuilder<A, O> builder
    ) where B : A =>
      builder(obs => multipleFinishes(obs, 1 + othersCount, checkFinished => {
        var selfSub = subscribe(obs.push, checkFinished);
        var otherSubs = others.Select(bObs =>
          bObs.subscribe(b => obs.push(b), checkFinished)
        );
        return selfSub.joinEnum(otherSubs);
      }));

    #endregion

    #region #joinDiscard

    public IObservable<Unit> joinDiscard<X>(IObservable<X> other) => 
      joinDiscardImpl(other, builder<Unit>());

    protected O joinDiscardImpl<X, O>
    (IObservable<X> other, ObserverBuilder<Unit, O> builder) => 
      builder(obs => multipleFinishes(obs, 2, checkFinished => 
        subscribe(
          _ => obs.push(F.unit),
          checkFinished
        ).join(other.subscribe(
          v => obs.push(F.unit),
          checkFinished
        ))
      ));

    #endregion

    #region #onceEvery

    // TODO: test with integration tests
    public IObservable<A> onceEvery(Duration duration, TimeScale timeScale) {
      return onceEveryImpl(duration, timeScale, builder<A>());
    }

    protected O onceEveryImpl<O>(
      Duration duration, TimeScale timeScale, ObserverBuilder<A, O> builder
    ) => builder(obs => {
      var lastEmit = float.NegativeInfinity;
      return subscribe(
        value => {
          var now = timeScale.now();
          if (lastEmit + duration.seconds > now) return;
          lastEmit = now;
          obs.push(value);
        },
        obs.finish
      );
    });

    #endregion

    #region #withinTimeframe

    public IObservable<ReadOnlyLinkedList<Tpl<A, float>>> 
    withinTimeframe(int count, Duration timeframe, TimeScale timeScale) => 
      withinTimeframeImpl(
        count, timeframe, timeScale, builder<ReadOnlyLinkedList<Tpl<A, float>>>()
      );

    protected O withinTimeframeImpl<O>(
      int count, Duration timeframe, TimeScale timeScale, 
      ObserverBuilder<ReadOnlyLinkedList<Tpl<A, float>>, O> builder
    ) => builder(obs => 
      map(value => F.t(value, timeScale.now())).
      buffer(count).
      filter(events => {
        if (events.Count != count) return false;
        var last = events.Last.Value._2;

        return events.All(t => last - t._2 <= timeframe.seconds);
      }).subscribe(obs)
    );

    #endregion

    #region #delayed

    public IObservable<A> delayed(float seconds) => delayedImpl(seconds, builder<A>());

    // TODO: test, but how? Can't do async tests in unity.
    protected O delayedImpl<O>(
      float seconds, ObserverBuilder<A, O> builder
    ) => builder(obs => subscribe(
      v => ASync.WithDelay(seconds, () => obs.push(v)),
      () => ASync.WithDelay(seconds, obs.finish)
    ));

    #endregion

    #region Zip

    public IObservable<Tpl<A, B>> zip<B>(IObservable<B> other) => 
      zipImpl(other, builder<Tpl<A, B>>());

    protected O zipImpl<B, O>
    (IObservable<B> other, ObserverBuilder<Tpl<A, B>, O> builder) => 
      builder(obs => multipleFinishes(obs, 2, checkFinish => {
        var lastSelf = F.none<A>();
        var lastOther = F.none<B>();
        Action notify = () => {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastOther)
            obs.push(F.t(aVal, bVal));
        };
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = other.subscribe(val => { lastOther = F.some(val); notify(); }, checkFinish);
        return s1.join(s2);
      }));

    public IObservable<Tpl<A, B, C>> zip<B, C>(IObservable<B> o1, IObservable<C> o2) => 
      zipImpl(o1, o2, builder<Tpl<A, B, C>>());

    protected O zipImpl<B, C, O>
    (IObservable<B> o1, IObservable<C> o2, ObserverBuilder<Tpl<A, B, C>, O> builder) {
      return builder(obs => multipleFinishes(obs, 3, checkFinish => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        Action notify = () => {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastO1)
          foreach (var cVal in lastO2)
            obs.push(F.t(aVal, bVal, cVal));
        };
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3);
      }));
    }

    public IObservable<Tpl<A, B, C, D>> zip<B, C, D>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3
    ) => zipImpl(o1, o2, o3, builder<Tpl<A, B, C, D>>());

    protected O zipImpl<B, C, D, O>
    (IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, ObserverBuilder<Tpl<A, B, C, D>, O> builder) {
      return builder(obs => multipleFinishes(obs, 4, checkFinish => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        var lastO3 = F.none<D>();
        Action notify = () => {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastO1)
          foreach (var cVal in lastO2)
          foreach (var dVal in lastO3)
            obs.push(F.t(aVal, bVal, cVal, dVal));
        };
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3, s4);
      }));
    }

    public IObservable<Tpl<A, B, C, D, E>> zip<B, C, D, E>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4
    ) => zipImpl(o1, o2, o3, o4, builder<Tpl<A, B, C, D, E>>());

    protected O zipImpl<B, C, D, E, O>(
      IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4, 
      ObserverBuilder<Tpl<A, B, C, D, E>, O> builder
    ) {
      return builder(obs => multipleFinishes(obs, 5, checkFinish => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        var lastO3 = F.none<D>();
        var lastO4 = F.none<E>();
        Action notify = () => {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastO1)
          foreach (var cVal in lastO2)
          foreach (var dVal in lastO3)
          foreach (var eVal in lastO4)
            obs.push(F.t(aVal, bVal, cVal, dVal, eVal));
        };
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); }, checkFinish);
        var s5 = o4.subscribe(val => { lastO4 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3, s4, s5);
      }));
    }

    public IObservable<Tpl<A, A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, IObservable<A4> o4, IObservable<A5> o5
    ) => zipImpl(o1, o2, o3, o4, o5, builder<Tpl<A, A1, A2, A3, A4, A5>>());

    protected O zipImpl<A1, A2, A3, A4, A5, O>(
      IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, IObservable<A4> o4, IObservable<A5> o5,
      ObserverBuilder<Tpl<A, A1, A2, A3, A4, A5>, O> builder
    ) {
      return builder(obs => multipleFinishes(obs, 6, checkFinish => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<A1>();
        var lastO2 = F.none<A2>();
        var lastO3 = F.none<A3>();
        var lastO4 = F.none<A4>();
        var lastO5 = F.none<A5>();
        Action notify = () => {
          foreach (var aVal in lastSelf)
          foreach (var a1Val in lastO1)
          foreach (var a2Val in lastO2)
          foreach (var a3Val in lastO3)
          foreach (var a4Val in lastO4)
          foreach (var a5Val in lastO5)
            obs.push(F.t(aVal, a1Val, a2Val, a3Val, a4Val, a5Val));
        };
        var s1 = subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); }, checkFinish);
        var s5 = o4.subscribe(val => { lastO4 = F.some(val); notify(); }, checkFinish);
        var s6 = o5.subscribe(val => { lastO5 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3, s4, s5, s6);
      }));
    }

    #endregion

    #region Changes

    O changesBase<Elem, O>(
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

    public IObservable<Tpl<Option<A>, A>> changesOpt(Fn<A, A, bool> areEqual=null) {
      return changesOptImpl(areEqual ?? EqComparer<A>.Default.Equals, builder<Tpl<Option<A>, A>>());
    }

    protected O changesOptImpl<O>(Fn<A, A, bool> areEqual, ObserverBuilder<Tpl<Option<A>, A>, O> builder) {
      return changesBase((obs, lastValue, val) => {
        var valueChanged = lastValue.fold(
          () => true,
          lastVal => !areEqual(lastVal, val)
        );
        if (valueChanged) obs.push(F.t(lastValue, val));
      }, builder);
    }

    public IObservable<Tpl<A, A>> changes(Fn<A, A, bool> areEqual=null) {
      return changesImpl(areEqual ?? EqComparer<A>.Default.Equals, builder<Tpl<A, A>>());
    }

    protected O changesImpl<O>(Fn<A, A, bool> areEqual, ObserverBuilder<Tpl<A, A>, O> builder) {
      return changesBase((obs, lastValue, val) => {
        foreach (var lastVal in lastValue) 
          if (! areEqual(lastVal, val))
            obs.push(F.t(lastVal, val));
      }, builder);
    }

    public IObservable<A> changedValues(Fn<A, A, bool> areEqual=null) => 
      changedValuesImpl(areEqual ?? EqComparer<A>.Default.Equals, builder<A>());

    protected O changedValuesImpl<O>(Fn<A, A, bool> areEqual, ObserverBuilder<A, O> builder) {
      return changesBase((obs, lastValue, val) => {
        if (lastValue.isEmpty) obs.push(val);
        else if (! areEqual(lastValue.get, val))
          obs.push(val);
      }, builder);
    }

    #endregion

    #region #oncePerFrame

    // TODO: test, but how? Can't do async tests in unity.
    public IObservable<A> oncePerFrame() => oncePerFrameImpl(builder<A>());

    protected O oncePerFrameImpl<O>(ObserverBuilder<A, O> builder) {
      return builder(obs => {
        var last = F.none<A>();
        var mySub = subscribe(v => last = v.some(), obs.finish);
        var luSub = ASync.onLateUpdate.subscribe(_ => {
          foreach (var val in last) { 
            // Clear last before pushing, because exception makes it loop forever.
            last = new Option<A>();
            obs.push(val);
          }
        });
        return mySub.join(luSub);
      });
    }

    #endregion

    #region #skip

    public IObservable<A> skip(uint count) => skipImpl(count, builder<A>());

    protected O skipImpl<O>(uint count, ObserverBuilder<A, O> builder) {
      return builder(obs => {
        var skipped = 0u;
        return subscribe(
          a => {
            if (skipped < count) skipped++;
            else obs.push(a);
          },
          obs.finish
        );
      });
    }

    #endregion

    public IRxVal<A> toRxVal(A initial) => RxVal.a(initial, subscribe);

    public IObservable<A> asObservable => this;

    public IObservable<A> setLogging(bool value) {
      doLogging = value;
      return this;
    }

    public static Ret multipleFinishes<B, Ret>(IObserver<B> obs, int count, Fn<Action, Ret> f) {
      var finished = 0;
      Action checkFinish = () => {
        finished++;
        if (finished == count) obs.finish();
      };
      return f(checkFinish);
    }

    #region private methods

    void subscribeToSource(SourceProperties source) {
      if (source.trySubscribe()) log("subscribed to source");
    }

    void onUnsubscribed() {
      pendingRemovals++;
      if (iterating) return;
      afterIteration();

      // Unsubscribe from source if we don't have any subscribers that are
      // subscribed to us.
      foreach (var source in sourceProps) {
        if (subscribers == 0 && !source.beAlwaysSubscribed) {
          if (source.tryUnsubscribe()) log("unsubscribed from source");
        }
      }
    }

    void log(string s) {
      if (doLogging && Log.isVerbose) Log.verbose($"[{nameof(Observable<A>)}] {s}");
    }

    void afterIteration() {
      if (pendingSubscriptionActivations != 0) {
        for (var idx = 0; idx < subscriptions.Count; idx++) {
          var sub = subscriptions[idx];
          if (!sub.active) {
            sub.active = true;
            // sub is a mutable struct, so we need to assign it back.
            subscriptions[idx] = sub;
          }
        }
        pendingSubscriptionActivations = 0;
      }
      if (pendingRemovals != 0) {
        subscriptions.RemoveWhere(sub => !sub.subscription.isSubscribed);
        pendingRemovals = 0;
      }
      if (willFinish) {
        willFinish = false;
        finishObservable();
      }
    }

    #endregion
  }

  public interface IObservableQueue<A, out C> {
    void addLast(A a);
    A first { get; }
    A last { get; }
    void removeFirst();
    int count { get; }
    C collection { get; }
  }

  public class ObservableReadOnlyLinkedListQueue<A> 
    : IObservableQueue<A, ReadOnlyLinkedList<A>>
  {
    readonly LinkedList<A> buffer;
    public ReadOnlyLinkedList<A> collection { get; }

    public ObservableReadOnlyLinkedListQueue() {
      buffer = new LinkedList<A>();
      collection = new ReadOnlyLinkedList<A>(buffer);
    }

    public void addLast(A a) => buffer.AddLast(a);
    public void removeFirst() => buffer.RemoveFirst();
    public int count => buffer.Count;
    public A first => buffer.First.Value;
    public A last => buffer.Last.Value;
  }

  public class ObservableLambdaQueue<A, C>
    : IObservableQueue<A, C>
  {
    readonly Act<A> _addLast;
    readonly Action _removeFirst;
    readonly Fn<int> _count;
    readonly Fn<C> _collection;
    readonly Fn<A> _first, _last;

    public ObservableLambdaQueue(Act<A> addLast, Action removeFirst, Fn<int> count, Fn<C> collection, Fn<A> first, Fn<A> last) {
      _addLast = addLast;
      _removeFirst = removeFirst;
      _count = count;
      _collection = collection;
      _first = first;
      _last = last;
    }

    public void addLast(A a) => _addLast(a);
    public void removeFirst() => _removeFirst();
    public int count => _count();
    public C collection => _collection();
    public A first => _first();
    public A last => _last();
  }
}