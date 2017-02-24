using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
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
    /** Return self as IObservable. */
    IObservable<A> asObservable { get; }
    // Logs actions at verbose level to standard logger.
    IObservable<A> setLogging(bool value);
  }

  public static class Observable {
    public static IObservable<A> a<A>(SubscribeFn<A> subscribeFn) => 
      new Observable<A>(subscribeFn);

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

  public delegate ISubscription SubscribeFn<out Elem>(IObserver<Elem> observer);

  public delegate ObservableImplementation ObserverBuilder<
    in Elem, out ObservableImplementation
  >(SubscribeFn<Elem> subscriptionFn);

  public class ObservableFinishedException : Exception {
    public ObservableFinishedException(string message) : base(message) {}
  }

  public class Observable<A> : IObservable<A> {
    public static readonly Observable<A> empty =
      new Observable<A>(_ => Subscription.empty);

    /** Properties if this observable was created from other source. **/
    class SourceProperties {
      readonly IObserver<A> observer;
      readonly SubscribeFn<A> subscribeFn;
      public readonly bool beAlwaysSubscribed;

      Option<ISubscription> subscription = F.none<ISubscription>();

      public SourceProperties(
        IObserver<A> observer, SubscribeFn<A> subscribeFn, 
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

    struct Sub {
      public readonly Subscription subscription;
      public readonly IObserver<A> observer;
      public readonly bool active;

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

      public Sub withActive(bool active) =>
        new Sub(subscription, observer, active);
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
      SubscribeFn<A> subscribeFn,
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
      if (finished) return Subscription.empty;

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

    public IObservable<A> asObservable => this;

    public IObservable<A> setLogging(bool value) {
      doLogging = value;
      return this;
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
          if (!sub.active) subscriptions[idx] = sub.withActive(true);
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
}