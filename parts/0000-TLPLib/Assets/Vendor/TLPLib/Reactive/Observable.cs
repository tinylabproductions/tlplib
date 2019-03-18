using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * Notes:
   *
   * #subscribe - if you subscribe to an observable during a callback, you
   * will not get the current event.
   *
   * <code>
   * void example(IRxObservable<A> observable) {
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
  public interface IRxObservable {
    int subscribers { get; }
  }

  public interface IRxObservable<out A> : IRxObservable {
    /// <summary>
    /// Subscribe to this observable to get a value every time an event happens.
    ///
    /// Before using this you need to understand how lifetime of subscriptions and observables
    /// work.
    ///
    /// This diagram shows types of references that exist between various objects in observables.
    ///
    /// <code><![CDATA[
    ///       +--------------+
    ///       | Subscription |
    ///       +--------------+
    ///         /|\       |
    ///          |        |
    ///          |        |
    ///          |       \|/
    ///    +------------------+        +-------------+        +------------------------------+
    ///    |    Observable    |------->| Action Code |------->| Object to perform effects on |
    ///    +------------------+        +-------------+        +------------------------------+
    /// ]]></code>
    ///
    /// Observables are divided into two major groups: sources and transformations.
    ///
    /// ### Sources
    ///
    /// Sources are things, where events originate (for example <see cref="Subject{A}"/>). They
    /// are usually referenced with a hard reference in some other object and emit events when
    /// things happen in Unity, for example:
    ///
    /// <code><![CDATA[
    /// public class OnUpdateForwarder : MonoBehaviour, IMB_Update {
    ///   readonly Subject<Unit> _onUpdate = new Subject<Unit>();
    ///   public IRxObservable<Unit> onUpdate => _onUpdate;
    ///
    ///   public void Update() => _onUpdate.push(F.unit);
    /// }
    /// ]]></code>
    ///
    /// They are garbage collected when the underlying MonoBehaviour is collected.
    ///
    /// ### Transformations
    ///
    /// Transformations are observables that emit events based on one or more source observables.
    ///
    /// For example <see cref="ObservableOps.zip{A1,A2,R}(IRxObservable,IRxObservable,System.Fn{A1,A2,R})"/>
    /// takes two observables and produces an observable that emits tupled values.
    ///
    /// Transformations have a nice property, that if nobody is listening to them, they do not have to listen
    /// to their source as well.
    ///
    /// This allows them to only run the transformation code in case they have listeners subscribed.
    ///
    /// However this also implies that they hold a hard reference to their source, because they need to be
    /// able to subscribe or unsubscribe to their source at any time.
    ///
    /// Lets look into how memory layout looks for them.
    ///
    /// These observables are often not explicitly referenced, for example:
    ///
    /// <code><![CDATA[
    /// this.someEventSource =
    ///   observableA.zip(observableB)
    ///   .map(t => Mathf.max(t._1, t._2))
    ///   .filter(_ => _ > 10);
    /// ]]></code>
    ///
    /// Here zip and map are not explicitly referenced, however each operation creates an intermediate
    /// observable. The memory layout looks like this.
    ///
    /// <code><![CDATA[
    ///                   source hard ref
    ///                 /-----------------
    ///                |/                 \
    ///   +-------------+              +----------------+ source   +----------------+
    ///   | observableA |              | zip observable |<---------| map observable |
    ///   +-------------+              +----------------+ hard ref +----------------+
    ///                                   /                                /|\
    ///                 /-----------------                           source | hard ref
    ///                |/ source hard ref                                   |
    ///   +-------------+                                          +-------------------+
    ///   | observableB |                                          | filter observable |
    ///   +-------------+                                          +-------------------+
    ///                                                                    /|\
    ///                                                            hard ref | this.someEventSource
    ///                                                                     |
    ///                                                                 +-------+
    ///                                                                 | this  |
    /// ]]></code>                                                      +-------+
    ///
    /// As you can see, all the references point backwards and the only thing that is keeping the whole
    /// thing from being garbage collected is a hard reference from this. If this were to be collected, so would
    /// be the whole observable chain.
    ///
    /// On the other hand if observableA and observableB are sources and no one has references to them, new
    /// events can not be emmited and the whole thing is kept in memory for no reason.
    ///
    /// Unfortunately this is a design limitation.
    ///
    /// You can either:
    /// 1. have references pointing backwards. You do not perform calculations when no one is listening, however
    ///    you cannot garbage collect when no one can emit an event (references to sources are lost).
    /// 2. have references pointing forwards. You do perform calculations even if no one is listening, however
    ///    you can garbage collect if all references to sources are lost.
    ///
    /// We feel that option 1 is more likely in day to day code.
    ///
    /// If you subscribe to this.someEventSource, it subscribes to map observable, which subscribes
    /// to zip, which then in turn subscribes to observableA and observableB, which are source, not transformation
    /// observables.
    ///
    /// The memory layout then looks like this (action objects between observables ommited for brevity).
    ///
    /// <code><![CDATA[
    ///                   source hard ref              act hard ref
    ///                 /-----------------            -----------------\
    ///                |/                 \          /                 \|
    ///   +-------------+ act hard ref +----------------+ source   +----------------+
    ///   | observableA |------------->| zip observable |<---------| map observable |-------\
    ///   +-------------+              +----------------+ hard ref +----------------+       | act
    ///                                   /  /|\                           /|\              | hard
    ///                 /-----------------    |                      source | hard ref      | ref
    ///                |/ source hard ref     |                             |               |
    ///   +-------------+                     |    act hard ref    +-------------------+    |
    ///   | observableB |--------------------/   /-----------------| filter observable |<---/
    ///   +-------------+  act hard ref          |           _____ +-------------------+
    ///                                          |          |     /|\      /|\
    ///                                          |          |      |        |  hard ref
    ///                                         \|/         |      |        |  this.someEventSource
    ///                               +--------------+      |      |    +-------+
    ///                               | Subscription |      |      |    | this  |
    ///                               | action code  |      |      |    +-------+
    ///                               +--------------+     \|/     |
    ///                                                 +--------------+
    ///                                                 | Subscription |
    ///                                                 +--------------+
    /// ]]></code>
    ///
    /// ### Closing points
    ///
    /// When using observables we need to ensure we do not leak memory.
    ///
    /// The easiest way to do that is to force a user to give an <see cref="IDisposableTracker"/> that is
    /// responsible for cleaning up the subscription when the object on which the subscription action works
    /// is destroyed.
    ///
    /// For caller information please refer to
    /// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/caller-information
    /// </summary>
    ISubscription subscribe(
      IDisposableTracker tracker, Action<A> onEvent,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    );

    void subscribe(
      IDisposableTracker tracker, Action<A> onEvent, out ISubscription subscription,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    );
  }

  public static class Observable {
    public static IObservableQueue<A, C> createQueue<A, C>(
      Action<A> addLast, Action removeFirst,
      Func<int> count, Func<C> collection, Func<A> first, Func<A> last
    ) => new ObservableLambdaQueue<A, C>(
      addLast, removeFirst, count, collection, first, last
    );

    public static Tpl<A, IRxObservable<Evt>> a<A, Evt>(
      Func<Action<Evt>, Tpl<A, ISubscription>> creator
    ) {
      Action<Evt> observer = null;
      ISubscription subscription = null;
      var observable = new Observable<Evt>(obs => {
        observer = obs;
        return subscription;
      });
      var t = creator(observer);
      var obj = t._1;
      subscription = t._2;
      return F.t(obj, (IRxObservable<Evt>) observable);
    }

    public static IRxObservable<A> empty<A>() => Observable<A>.empty;

    public static IRxObservable<A> fromEvent<A>(
      Action<Action<A>> registerCallback, Action unregisterCallback
    ) {
      return new Observable<A>(obs => {
        registerCallback(obs);
        return new Subscription(unregisterCallback);
      });
    }

    static IRxObservable<Unit> everyFrameInstance;

    public static IRxObservable<Unit> everyFrame =>
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

    static IRxObservable<List<Touch>> touchesInstance;

    public static IRxObservable<List<Touch>> touches =>
      touchesInstance ?? (touchesInstance = createTouchesInstance());

    static IRxObservable<List<Touch>> createTouchesInstance() {
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

    public static IRxObservable<DateTime> interval(Duration interval, Duration delay) =>
      Observable.interval(interval, F.some(delay));

    public static IRxObservable<DateTime> interval(
      Duration interval, Option<Duration> delay=default(Option<Duration>)
    ) {
      Option.ensureValue(ref delay);
      return new Observable<DateTime>(observer => {
        var cr = ASync.StartCoroutine(intervalEnum(observer, interval, delay));
        return new Subscription(cr.stop);
      });
    }

    static IEnumerator everyFrameCR(Action<Unit> onEvent) {
      while (true) {
        onEvent(Unit.instance);
        yield return null;
      }
      // ReSharper disable once IteratorNeverReturns
    }

    static IEnumerator intervalEnum(
      Action<DateTime> pushEvent, Duration interval, Option<Duration> delay
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

  // This is here, instead of in Observable<A>, because il2cpp generates nonsense code
  // when the delegate is in the class.
  // ReSharper disable once TypeParameterCanBeVariant
  public delegate ISubscription SubscribeToSource<A>(Action<A> onEvent);

  public partial class Observable<A> : IRxObservable<A> {
    public static readonly Observable<A> empty =
      new Observable<A>(_ => Subscription.empty);

    /** Properties if this observable was created from other source. **/
    class SourceProperties {
      readonly Action<A> onEvent;
      readonly SubscribeToSource<A> subscribeFn;

      // not an option due to that option is reference type on il2cpp
      ISubscription subscription;

      public SourceProperties(
        Action<A> onEvent, SubscribeToSource<A> subscribeFn
      ) {
        this.onEvent = onEvent;
        this.subscribeFn = subscribeFn;
      }

      public bool trySubscribe() {
        if (subscription == null) {
          subscription = subscribeFn(onEvent);
          return true;
        }
        return false;
      }

      public bool tryUnsubscribe() {
        if (subscription != null) {
          var sub = subscription;
          subscription = null;
          return sub.unsubscribe();
        }
        return false;
      }
    }

    [Record]
    partial struct Sub {
      public readonly Action<A> onEvent;
      // When subscriptions happen whilst we are processing other event, they are
      // initially inactive.
      public readonly bool active;

      readonly bool haveUnsubscribed;

      // LEGACY_OBSERVABLES define makes hard references from source to subscription instead of default weak references
      // we use this mode in Gummy Bear to avoid major refactoring
#if LEGACY_OBSERVABLES
      readonly Subscription subscription;
#else
      readonly WeakReference<Subscription> subscription;
#endif
      public readonly string callerMemberName, callerFilePath;
      public readonly int callerLineNumber;

      public bool isSubscribed(out bool isBroken) {
        isBroken = false;
        if (haveUnsubscribed) return false;
#if LEGACY_OBSERVABLES
        return subscription.isSubscribed;
#else
        if (subscription.TryGetTarget(out var sub)) return sub.isSubscribed;
        Log.d.error(
          $"Active subscription was garbage collected! You should always properly track your subscriptions. " +
          subscribedFrom
        );
        isBroken = true;
        return false;
#endif
      }

      public string subscribedFrom => $"Subscribed from {callerMemberName} @ {callerFilePath}:{callerLineNumber}.";

      public Sub withActive(bool active) => new Sub(
        onEvent: onEvent, active: active, haveUnsubscribed: haveUnsubscribed, subscription: subscription,
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
      );

      public Sub unsubscribe() => new Sub(
        onEvent: onEvent, active: false, haveUnsubscribed: true, subscription: subscription,
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
      );
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

    protected void submit(A a) {
      if (iterating) {
        // Do not submit if iterating.
        pendingSubmits.add(a);
        return;
      }

      // Mark a flag to prevent concurrent modification of subscriptions array.
      iterating = true;
      // Did we detect broken subscriptions?
      var brokenSubsDetected = false;
      try {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < subscriptions.Count; idx++) {
          var sub = subscriptions[idx];
          if (sub.active) {
            if (sub.isSubscribed(out var isBroken)) {
              try {
                sub.onEvent(a);
              }
              catch (Exception e) {
                Log.d.error("Exception on event, unsubscribing! " + sub.subscribedFrom, e);
                unsubscribe(sub, idx);
              }
            }

            brokenSubsDetected = brokenSubsDetected || isBroken;
          }
        }
      }
      finally {
        iterating = false;
        afterIteration(brokenSubsDetected);
        // Process pending submits.
        if (pendingSubmits.size > 0) submit(pendingSubmits.removeAt(0));
      }
    }

    public int subscribers => subscriptions.Count - pendingSubscriptionActivations - pendingRemovals;

    public virtual void subscribe(
      IDisposableTracker tracker, Action<A> onEvent, out ISubscription subscription,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      // Hard ref from subscription to this
      var sub = new Subscription(() => unsubscribe(onEvent));
      subscription = sub;
      tracker.track(
        subscription,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );

      var active = !iterating;
      subscriptions.Add(new Sub(
        onEvent: onEvent, active: active, haveUnsubscribed: false,
#if LEGACY_OBSERVABLES
        subscription: sub,
#else
        subscription: new WeakReference<Subscription>(sub),
#endif
        callerMemberName: callerMemberName, callerFilePath: callerFilePath,
        callerLineNumber: callerLineNumber
      ));
      if (!active) pendingSubscriptionActivations++;

      // Subscribe to source if we have a first subscriber.
      foreach (var source in sourceProps)
        source.trySubscribe();
    }

    public ISubscription subscribe(
      IDisposableTracker tracker, Action<A> onEvent,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      subscribe(
        tracker, onEvent, out var subscription,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );
      return subscription;
    }

    #region private methods

    void unsubscribe(Action<A> onEvent) {
      for (var idx = 0; idx < subscriptions.Count; idx++) {
        var sub = subscriptions[idx];
        if (sub.onEvent == onEvent) {
          subscriptions[idx] = sub.unsubscribe();
          unsubscribe(sub, idx);
          return;
        }
      }
    }

    void unsubscribe(Sub sub, int idx) {
      subscriptions[idx] = sub.unsubscribe();
      pendingRemovals++;
      if (iterating) return;
      afterIteration(false);

      // Unsubscribe from source if we don't have any subscribers that are
      // subscribed to us.
      foreach (var source in sourceProps) {
        if (subscribers == 0)
          source.tryUnsubscribe();
      }
    }

    void afterIteration(bool brokenSubsDetected) {
      if (pendingSubscriptionActivations != 0) {
        for (var idx = 0; idx < subscriptions.Count; idx++) {
          var sub = subscriptions[idx];
          if (!sub.active) subscriptions[idx] = sub.withActive(true);
        }
        pendingSubscriptionActivations = 0;
      }
      if (brokenSubsDetected || pendingRemovals != 0) {
        subscriptions.RemoveWhere(sub => !sub.isSubscribed(out _));
        pendingRemovals = 0;
      }
    }

    #endregion
  }
}