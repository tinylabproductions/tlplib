using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.system;
using UnityEngine;
using WeakReference = com.tinylabproductions.TLPLib.system.WeakReference;

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
   *     Log.d.info("A " + a);
   *     observable.subscribe(a1 => {
   *       Log.d.info("A1 " + a);
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
   *     Log.d.info("A1=" + a);
   *     if (a == 0) observable.push(a + 1);
   *   });
   *   observable.subscribe(a => {
   *     Log.d.info("A2=" + a);
   *   });
   *   observable.push(0);
   * }
   * </code>
   * 
   * Will print A1=0,A2=0 and then A1=1,A2=1, not A1=0,A1=1,A2=1,A2=0
   **/
  public interface IObservable {
    int subscribers { get; }
  }

  public interface IObservable<out A> : IObservable {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tracker">Tracker</param>
    /// <param name="onEvent"></param>
    ISubscription subscribe(IDisposableTracker tracker, Act<A> onEvent);
  }

  public static class Observable {
    public static IObservable<A> a<A>(SubscribeToSource<A> subscribeFn) => 
      new Observable<A>(subscribeFn);

    public static IObservableQueue<A, C> createQueue<A, C>(
      Act<A> addLast, Action removeFirst,
      Fn<int> count, Fn<C> collection, Fn<A> first, Fn<A> last
    ) => new ObservableLambdaQueue<A, C>(
      addLast, removeFirst, count, collection, first, last
    );

    public static Tpl<A, IObservable<Evt>> a<A, Evt>(
      Fn<Act<Evt>, Tpl<A, ISubscription>> creator
    ) {
      Act<Evt> observer = null;
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
        registerCallback(obs);
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

    static IEnumerator everyFrameCR(Act<Unit> onEvent) {
      while (true) {
        onEvent(Unit.instance);
        yield return null;
      }
      // ReSharper disable once IteratorNeverReturns
    }

    static IEnumerator intervalEnum(
      Act<DateTime> pushEvent, Duration interval, Option<Duration> delay
    ) {
      foreach (var d in delay) yield return new WaitForSeconds(d.seconds);
      var wait = new WaitForSeconds(interval.seconds);
      while (true) {
        pushEvent(DateTime.Now);
        yield return wait;
      }
      // ReSharper disable once IteratorNeverReturns
    }
  }

  public delegate ISubscription SubscribeToSource<out A>(Act<A> onEvent);

  public delegate ObservableImplementation ObserverBuilder<
    in Elem, out ObservableImplementation
  >(SubscribeToSource<Elem> subscriptionFn);

  public class Observable<A> : IObservable<A> {
    public static readonly Observable<A> empty =
      new Observable<A>(_ => Subscription.empty);

    /** Properties if this observable was created from other source. **/
    class SourceProperties {
      readonly Act<A> onEvent;
      readonly SubscribeToSource<A> subscribeFn;

      Option<ISubscription> subscription = F.none<ISubscription>();

      public SourceProperties(
        Act<A> onEvent, SubscribeToSource<A> subscribeFn
      ) {
        this.onEvent = onEvent;
        this.subscribeFn = subscribeFn;
      }

      public bool trySubscribe() {
        if (subscription.isNone) {
          subscription = F.some(subscribeFn(onEvent));
          return true;
        }
        return false;
      }

      public bool tryUnsubscribe() {
        foreach (var sub in subscription) {
          subscription = Option<ISubscription>.None;
          return sub.unsubscribe();
        }
        return false;
      }
    }

    struct Sub {
      public readonly Subscription subscription;
      public readonly WeakReference<Act<A>> onEvent;
      // When subscriptions happen whilst we are processing other event, they are
      // initially inactive.
      public readonly bool active;

      public Sub(Subscription subscription, WeakReference<Act<A>> onEvent, bool active) {
        this.subscription = subscription;
        this.onEvent = onEvent;
        this.active = active;
      }

      public override string ToString() => 
        $"{nameof(Sub)}[" +
        $"{nameof(subscription)}: {subscription}, " +
        $"{nameof(onEvent)}: {onEvent}, " +
        $"{nameof(active)}: {active}" +
        $"]";

      public Sub withActive(bool active) =>
        new Sub(subscription, onEvent, active);
    }

    readonly RandomList<Sub> subscriptions = new RandomList<Sub>();
    SList4<A> pendingSubmits = new SList4<A>();

    // Are we currently iterating through subscriptions?
    bool iterating;
    // How many subscription activations do we have pending?
    int pendingSubscriptionActivations;
    // How many subscription removals we have pending?
    int pendingRemovals;

    readonly Option<SourceProperties> sourceProps;

    protected Observable() {
      sourceProps = F.none<SourceProperties>();
    }

    public Observable(SubscribeToSource<A> subscribeFn) {
      sourceProps = new SourceProperties(submit, subscribeFn).some();
    }

    protected virtual void submit(A a) {

      if (iterating) {
        // Do not submit if iterating.
        pendingSubmits.add(a);
        return;
      }

      // Mark a flag to prevent concurrent modification of subscriptions array.
      iterating = true;
      try {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < subscriptions.Count; idx++) {
          var sub = subscriptions[idx];
          if (sub.active && sub.subscription.isSubscribed)
            foreach (var onEvent in sub.onEvent.Target)
              onEvent(a);
        }
      }
      finally {
        iterating = false;
        afterIteration();
        // Process pending submits.
        if (pendingSubmits.size > 0) submit(pendingSubmits.removeAt(0));
      }
    }
    
    public int subscribers => subscriptions.Count - pendingSubscriptionActivations - pendingRemovals;

    public virtual ISubscription subscribe(IDisposableTracker tracker, Act<A> onEvent) {
      // Create a hard reference from subscription to observable, so it would
      // keep the observable alive as long as we have a reference to subscription.
      // 
      //                 hard reference
      //              /-------------------\
      //             \/                   |
      //    /------------------\      +-------+
      //    |    Observable    |      |  Sub  | 
      //    \------------------/      +-------+
      //
      var subscription = new Subscription(onUnsubscribed);
      var active = !iterating;
      // Create a weak reference from observable to an action.
      //
      // We do not want to keep performing side-effects on an object if we are only ones
      // that have the reference to that object.
      // 
      //    /------------------\ weak   +-------------+ hard   +------------------------------+
      //    |    Observable    |- - - ->| Action Code |------->| Object to perform effects on | 
      //    \------------------/ ref    +-------------+ ref    +------------------------------+
      //
      subscriptions.Add(new Sub(subscription, WeakReference.a(onEvent), active));
      if (!active) pendingSubscriptionActivations++;
      
      // Subscribe to source if we have a first subscriber.
      foreach (var source in sourceProps)
        source.trySubscribe();
      return subscription;
    }

    #region private methods

    void onUnsubscribed() {
      pendingRemovals++;
      if (iterating) return;
      afterIteration();

      // Unsubscribe from source if we don't have any subscribers that are
      // subscribed to us.
      foreach (var source in sourceProps) {
        if (subscribers == 0)
          source.tryUnsubscribe();
      }
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
    }

    #endregion
  }
}