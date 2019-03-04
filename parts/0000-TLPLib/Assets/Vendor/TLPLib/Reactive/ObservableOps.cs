using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.dispose;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using Smooth.Collections;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class ObservableOps {
    #region #subscribe

    [PublicAPI] public static ISubscription subscribe<A>(
      this IRxObservable<A> observable,
      IDisposableTracker tracker,
      Act<A> onEvent,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      // ReSharper disable once AccessToModifiedClosure
      observable.subscribe(
        tracker: tracker, onEvent: onEvent, subscription: out var subscription,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );
      return subscription;
    }

    public static ISubscription subscribe<A>(
      this IRxObservable<A> observable,
      IDisposableTracker tracker,
      Act<A, ISubscription> onChange,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      ISubscription subscription = null;
      // ReSharper disable once AccessToModifiedClosure
      observable.subscribe(
        tracker: tracker, onEvent: a => onChange(a, subscription), subscription: out subscription,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );
      return subscription;
    }

    [PublicAPI] public static ISubscription subscribe<A>(
      this IRxObservable<A> observable, GameObject tracker, Act<A> onEvent,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => observable.subscribe(
      tracker: tracker.asDisposableTracker(), onEvent: onEvent,
      // ReSharper disable ExplicitCallerInfoArgument
      callerMemberName: callerMemberName, callerFilePath: callerFilePath,
      callerLineNumber: callerLineNumber
      // ReSharper restore ExplicitCallerInfoArgument
    );

    [PublicAPI] public static void subscribeLast<A>(
      this IRxObservable<A> observable, ref IDisposable subscription, Act<A> onChange,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      subscription.Dispose();
      subscription = observable.subscribe(
        tracker: NoOpDisposableTracker.instance, onEvent: onChange,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath,
        callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );
    }

    public static void subscribeLast<A>(
      this IRxObservable<A> observable, ref ISubscription subscription, Act<A> onChange,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      subscription.unsubscribe();
      subscription = observable.subscribe(
        tracker: NoOpDisposableTracker.instance, onEvent: onChange,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath,
        callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );
    }

    public static ISubscription subscribeForOneEvent<A>(
      this IRxObservable<A> observable,
      IDisposableTracker tracker,
      Act<A> onEvent,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) => observable.subscribe(
      tracker: tracker,
      onChange: (a, sub) => {
        sub.unsubscribe();
        onEvent(a);
      },
      // ReSharper disable ExplicitCallerInfoArgument
      callerMemberName: callerMemberName, callerFilePath: callerFilePath,
      callerLineNumber: callerLineNumber
      // ReSharper restore ExplicitCallerInfoArgument
    );

    #endregion

    /// <summary>
    /// Return self as IObservable.
    /// </summary>
    public static IRxObservable<A> asObservable<A>(this IRxObservable<A> observable) => observable;

    /** Maps events coming from this observable. **/
    public static IRxObservable<B> map<A, B>(
      this IRxObservable<A> o, Fn<A, B> mapper
    ) => new Observable<B>(onEvent =>
      o.subscribe(NoOpDisposableTracker.instance, val => onEvent(mapper(val)))
    );

    #region #flatMap

    /// <summary>
    /// Maps events coming from this observable and emits all events contained
    /// in returned enumerable.
    /// </summary>
    public static IRxObservable<B> flatMap<A, B>(
      this IRxObservable<A> o, Fn<A, IEnumerable<B>> mapper
    ) => new Observable<B>(onEvent => o.subscribe(NoOpDisposableTracker.instance, val => {
      foreach (var b in mapper(val)) onEvent(b);
    }));

    /// <summary>
    /// Maps events coming from this observable and emits all events that are emitted
    /// by returned observable.
    /// </summary>
    public static IRxObservable<B> flatMap<A, B>(
      this IRxObservable<A> o, Fn<A, IRxObservable<B>> mapper
    ) => new Observable<B>(onBEvent => {
      var bSub = Subscription.empty;
      void unsubscribeFromB() => bSub.unsubscribe();

      void onAEvent(A val) {
        unsubscribeFromB();
        var bObs = mapper(val);
        bSub = bObs.subscribe(NoOpDisposableTracker.instance, onBEvent);
      }

      var aSub = o.subscribe(NoOpDisposableTracker.instance, onAEvent);
      return aSub.andThen(unsubscribeFromB);
    });

    /**
     * Maps events coming from this observable and emits events from returned futures.
     *
     * Does not emit value if future completes after observable is finished.
     **/
    public static IRxObservable<B> flatMap<A, B>(
      this IRxObservable<A> o, Fn<A, Future<B>> mapper
    ) => new Observable<B>(onEvent =>
      o.subscribe(NoOpDisposableTracker.instance, a => mapper(a).onComplete(onEvent))
    );

    /// <summary>
    /// Wait until future completes and start emmiting events from the created
    /// observable then.
    /// </summary>
    public static IRxObservable<B> flatMap<A, B>(
      this Future<A> future, Fn<A, IRxObservable<B>> mapper
    ) => future.map(mapper).extract();

    /// <summary>
    /// Abstracts the future away and returns an observable that starts emmiting
    /// events when the future completes with another observable.
    /// </summary>
    public static IRxObservable<A> extract<A>(
      this Future<IRxObservable<A>> future
    ) {
      // Saves onEvent when someone is subscribed to us, but the future has not yet
      // completed.
      var supposedToBeSubscribed = Option<Act<A>>.None;
      // Saves the actual subscription that is filled in when the future completes
      // so that we could unsubscribe from source if everyone unsubscribes from us.
      var currentSubscription = Subscription.empty;
      // Subscription that cleans all the state up.
      var onUnsubscribe = new Subscription(() => {
        // Unsubscribe from real observable that the future returned.
        currentSubscription.unsubscribe();
        // Clean up the event handler storage, so that things would not happen
        // if future completes while no one is subscribed to this.
        supposedToBeSubscribed = supposedToBeSubscribed.none;
      });

      // Allows us to lose the reference to the future.
      var lastFutureValue = future.value;
      future.onComplete(obs => {
        // This path deals with the scenario where we got a subscriber before the future
        // completed. Now the future has completed and we have to start proxying events.
        lastFutureValue = obs.some();

        // When this completes if there is someone subscribed to this observable
        // start proxying events.
        if (supposedToBeSubscribed.isSome) {
          var onEvent = supposedToBeSubscribed.__unsafeGetValue;
          currentSubscription = obs.subscribe(NoOpDisposableTracker.instance, onEvent);
        }
      });

      ISubscription subscribeToSource(Act<A> onEvent) {
        if (lastFutureValue.isSome) {
          // If somebody subscribed to us and the future was already completed.
          var obs = lastFutureValue.__unsafeGetValue;
          currentSubscription = obs.subscribe(NoOpDisposableTracker.instance, onEvent);
        }
        else {
          // If the future was not completed, mark that we are supposed to be subscribed
          // to the source when the future completes.
          supposedToBeSubscribed = F.some(onEvent);
        }

        return onUnsubscribe;
      }

      return new Observable<A>(subscribeToSource);
    }

    #endregion

    /** Only emits events that pass the predicate. **/
    public static IRxObservable<A> filter<A>(
      this IRxObservable<A> o, Fn<A, bool> predicate
    ) => new Observable<A>(onEvent =>
      o.subscribe(NoOpDisposableTracker.instance, val => { if (predicate(val)) onEvent(val); })
    );

    /// <summary>Emits given value upon first event to the future and unsubscribes.</summary>
    public static Future<B> toFuture<A, B>(this IRxObservable<A> o, B b) =>
      Future<B>.async((p, f) => {
        var subscription = o.subscribe(NoOpDisposableTracker.instance, _ => p.complete(b));
        f.onComplete(_ => subscription.unsubscribe());
      });

    /// <summary>Emits first value to the future and unsubscribes.</summary>
    public static Future<A> toFuture<A>(this IRxObservable<A> o) =>
      Future<A>.async((p, f) => {
        var subscription = o.subscribe(NoOpDisposableTracker.instance, p.complete);
        f.onComplete(_ => subscription.unsubscribe());
      });

    // Skips `count` values from the stream.
    public static IRxObservable<A> skip<A>(this IRxObservable<A> o, uint count) =>
      new Observable<A>(onEvent => {
        var skipped = 0u;
        return o.subscribe(NoOpDisposableTracker.instance, a => {
          if (skipped < count) skipped++;
          else onEvent(a);
        });
      });

    /// <summary>Emits n'th value to the future and unsubscribes.</summary>
    public static Future<A> toFuture<A>(this IObservable<A> o, uint n) => (n > 1 ? o.skip(n - 1) : o).toFuture();

    // If several events are emitted per same frame, only emit last one in late update.
    // TODO: test, but how? Can't do async tests in unity.
    public static IRxObservable<A> oncePerFrame<A>(this IRxObservable<A> o) =>
      new Observable<A>(onEvent => {
        var last = Option<A>.None;
        var mySub = o.subscribe(NoOpDisposableTracker.instance, v => last = v.some());
        var luSub = ASync.onLateUpdate.subscribe(NoOpDisposableTracker.instance, _ => {
          foreach (var val in last) {
            // Clear last before pushing, because exception makes it loop forever.
            last = Option<A>.None;
            onEvent(val);
          }
        });
        return mySub.join(luSub);
      });

    #region #zip

    public static IRxObservable<R> zip<A1, A2, R>(
      this IRxObservable<A1> o, IRxObservable<A2> other, Fn<A1, A2, R> zipper
    ) =>
      new Observable<R>(onEvent => {
        var lastSelf = F.none<A1>();
        var lastOther = F.none<A2>();

        void notify() {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastOther)
            onEvent(zipper(aVal, bVal));
        }

        var s1 = o.subscribe(NoOpDisposableTracker.instance, val => { lastSelf = F.some(val); notify(); });
        var s2 = other.subscribe(NoOpDisposableTracker.instance, val => { lastOther = F.some(val); notify(); });
        return s1.join(s2);
      });

    [Obsolete("Use zip with custom mapper")]
    public static IRxObservable<Tpl<A1, A2>> zip<A1, A2>(
      this IRxObservable<A1> o1, IRxObservable<A2> o2
    ) => o1.zip(o2, F.t);

    public static IRxObservable<R> zip<A1, A2, A3, R>(
      this IRxObservable<A1> o, IRxObservable<A2> o1, IRxObservable<A3> o2, Fn<A1, A2, A3, R> zipper
    ) => new Observable<R>(onEvent => {
      var lastSelf = F.none<A1>();
      var lastO1 = F.none<A2>();
      var lastO2 = F.none<A3>();

      void notify() {
        foreach (var aVal in lastSelf)
        foreach (var bVal in lastO1)
        foreach (var cVal in lastO2)
          onEvent(zipper(aVal, bVal, cVal));
      }

      var s1 = o.subscribe(NoOpDisposableTracker.instance, val => { lastSelf = F.some(val); notify(); });
      var s2 = o1.subscribe(NoOpDisposableTracker.instance, val => { lastO1 = F.some(val); notify(); });
      var s3 = o2.subscribe(NoOpDisposableTracker.instance, val => { lastO2 = F.some(val); notify(); });
      return s1.join(s2, s3);
    });

    [Obsolete("Use zip with custom mapper")]
    public static IRxObservable<Tpl<A1, A2, A3>> zip<A1, A2, A3>(
      this IRxObservable<A1> o1, IRxObservable<A2> o2, IRxObservable<A3> o3
    ) => o1.zip(o2, o3, F.t);

    public static IRxObservable<R> zip<A1, A2, A3, A4, R>(
      this IRxObservable<A1> o, IRxObservable<A2> o1, IRxObservable<A3> o2, IRxObservable<A4> o3,
      Fn<A1, A2, A3, A4, R> zipper
    ) => new Observable<R>(onEvent => {
      var lastSelf = F.none<A1>();
      var lastO1 = F.none<A2>();
      var lastO2 = F.none<A3>();
      var lastO3 = F.none<A4>();

      void notify() {
        foreach (var aVal in lastSelf)
        foreach (var bVal in lastO1)
        foreach (var cVal in lastO2)
        foreach (var dVal in lastO3)
          onEvent(zipper(aVal, bVal, cVal, dVal));
      }

      var s1 = o.subscribe(NoOpDisposableTracker.instance, val => { lastSelf = F.some(val); notify(); });
      var s2 = o1.subscribe(NoOpDisposableTracker.instance, val => { lastO1 = F.some(val); notify(); });
      var s3 = o2.subscribe(NoOpDisposableTracker.instance, val => { lastO2 = F.some(val); notify(); });
      var s4 = o3.subscribe(NoOpDisposableTracker.instance, val => { lastO3 = F.some(val); notify(); });
      return s1.join(s2, s3, s4);
    });

    [Obsolete("Use zip with custom mapper")]
    public static IRxObservable<Tpl<A1, A2, A3, A4>> zip<A1, A2, A3, A4>(
      this IRxObservable<A1> o1, IRxObservable<A2> o2, IRxObservable<A3> o3, IRxObservable<A4> o4
    ) => o1.zip(o2, o3, o4, F.t);

    public static IRxObservable<R> zip<A1, A2, A3, A4, A5, R>(
      this IRxObservable<A1> o, IRxObservable<A2> o1, IRxObservable<A3> o2, IRxObservable<A4> o3, IRxObservable<A5> o4,
      Fn<A1, A2, A3, A4, A5, R> zipper
    ) => new Observable<R>(onEvent => {
      var lastSelf = F.none<A1>();
      var lastO1 = F.none<A2>();
      var lastO2 = F.none<A3>();
      var lastO3 = F.none<A4>();
      var lastO4 = F.none<A5>();

      void notify() {
        foreach (var aVal in lastSelf)
        foreach (var bVal in lastO1)
        foreach (var cVal in lastO2)
        foreach (var dVal in lastO3)
        foreach (var eVal in lastO4)
          onEvent(zipper(aVal, bVal, cVal, dVal, eVal));
      }

      var s1 = o.subscribe(NoOpDisposableTracker.instance, val => { lastSelf = F.some(val); notify(); });
      var s2 = o1.subscribe(NoOpDisposableTracker.instance, val => { lastO1 = F.some(val); notify(); });
      var s3 = o2.subscribe(NoOpDisposableTracker.instance, val => { lastO2 = F.some(val); notify(); });
      var s4 = o3.subscribe(NoOpDisposableTracker.instance, val => { lastO3 = F.some(val); notify(); });
      var s5 = o4.subscribe(NoOpDisposableTracker.instance, val => { lastO4 = F.some(val); notify(); });
      return s1.join(s2, s3, s4, s5);
    });

    [Obsolete("Use zip with custom mapper")]
    public static IRxObservable<Tpl<A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      this IRxObservable<A1> o1, IRxObservable<A2> o2, IRxObservable<A3> o3, IRxObservable<A4> o4, IRxObservable<A5> o5
    ) => o1.zip(o2, o3, o4, o5, F.t);

    public static IRxObservable<R> zip<A, A1, A2, A3, A4, A5, R>(
      this IRxObservable<A> o, IRxObservable<A1> o1, IRxObservable<A2> o2, IRxObservable<A3> o3,
      IRxObservable<A4> o4, IRxObservable<A5> o5, Fn<A, A1, A2, A3, A4, A5, R> zipper
    ) => new Observable<R>(onEvent => {
      var lastSelf = F.none<A>();
      var lastO1 = F.none<A1>();
      var lastO2 = F.none<A2>();
      var lastO3 = F.none<A3>();
      var lastO4 = F.none<A4>();
      var lastO5 = F.none<A5>();

      void notify() {
        foreach (var aVal in lastSelf)
        foreach (var a1Val in lastO1)
        foreach (var a2Val in lastO2)
        foreach (var a3Val in lastO3)
        foreach (var a4Val in lastO4)
        foreach (var a5Val in lastO5)
          onEvent(zipper(aVal, a1Val, a2Val, a3Val, a4Val, a5Val));
      }

      var s1 = o.subscribe(NoOpDisposableTracker.instance, val => { lastSelf = F.some(val); notify(); });
      var s2 = o1.subscribe(NoOpDisposableTracker.instance, val => { lastO1 = F.some(val); notify(); });
      var s3 = o2.subscribe(NoOpDisposableTracker.instance, val => { lastO2 = F.some(val); notify(); });
      var s4 = o3.subscribe(NoOpDisposableTracker.instance, val => { lastO3 = F.some(val); notify(); });
      var s5 = o4.subscribe(NoOpDisposableTracker.instance, val => { lastO4 = F.some(val); notify(); });
      var s6 = o5.subscribe(NoOpDisposableTracker.instance, val => { lastO5 = F.some(val); notify(); });
      return s1.join(s2, s3, s4, s5, s6);
    });

    [Obsolete("Use zip with custom mapper")]
    public static IRxObservable<Tpl<A1, A2, A3, A4, A5, A6>> zip<A1, A2, A3, A4, A5, A6>(
      this IRxObservable<A1> o1, IRxObservable<A2> o2, IRxObservable<A3> o3, IRxObservable<A4> o4, IRxObservable<A5> o5,
      IRxObservable<A6> o6
    ) => o1.zip(o2, o3, o4, o5, o6, F.t);

    #endregion

    /// <summary>
    /// Discards values that this observable emits, turning it into event
    /// source that does not carry data with it.
    /// </summary>
    public static IRxObservable<Unit> discardValue<A>(this IRxObservable<A> o) =>
      new Observable<Unit>(onEvent =>
        o.subscribe(NoOpDisposableTracker.instance, _ => onEvent(F.unit))
      );

    /// <summary>
    /// Only emits events that return some.
    /// </summary>
    public static IRxObservable<B> collect<A, B>(
      this IRxObservable<A> o, Fn<A, Option<B>> collector
    ) => new Observable<B>(onEvent => o.subscribe(
      NoOpDisposableTracker.instance,
      val => {
        var opt = collector(val);
        if (opt.isSome) onEvent(opt.__unsafeGetValue);
      }
    ));

    #region #buffer

    /**
      * Buffers values into a linked list of specified size. Oldest values
      * are at the front of the buffer. Only emits `size` items at a time. When
      * new item arrives to the buffer, oldest one is removed.
      **/
    public static IRxObservable<ReadOnlyLinkedList<A>> buffer<A>(
      this IRxObservable<A> o, int size
    ) => o.buffer(size, new ObservableReadOnlyLinkedListQueue<A>());

    public static IRxObservable<C> buffer<A, C>(
      this IRxObservable<A> o, int size, IObservableQueue<A, C> queue
    ) => new Observable<C>(onEvent => o.subscribe(
      NoOpDisposableTracker.instance,
      val => {
        queue.addLast(val);
        if (queue.count > size) queue.removeFirst();
        onEvent(queue.collection);
      }
    ));

    #endregion

    #region #timeBuffer


    // TODO: TimeScale -> TimeContext & test
    /// <summary>
    /// Buffers values into a linked list for specified time period. Oldest values
    /// are at the front of the buffer. Emits tuples of (element, time).
    /// Only emits items if `duration` has passed. When
    /// new item arrives to the buffer, oldest one is removed.
    /// </summary>
    public static IRxObservable<ReadOnlyLinkedList<Tpl<A, float>>> timeBuffer<A>(
      this IRxObservable<A> o, Duration duration, TimeScale timeScale = TimeScale.Realtime
    ) => o.timeBuffer(duration, new ObservableReadOnlyLinkedListQueue<Tpl<A, float>>(), timeScale);

    public static IRxObservable<C> timeBuffer<A, C>(
      this IRxObservable<A> o, Duration duration, IObservableQueue<Tpl<A, float>, C> queue,
      TimeScale timeScale = TimeScale.Realtime
    ) => new Observable<C>(onEvent => o.subscribe(
      NoOpDisposableTracker.instance,
      val => {
        queue.addLast(F.t(val, timeScale.now()));
        var lastTime = queue.last._2;
        if (queue.first._2 + duration.seconds <= lastTime)
        {
          // Remove items which are too old.
          while (queue.first._2 + duration.seconds < lastTime)
            queue.removeFirst();
          onEvent(queue.collection);
        }
      }
    ));

    #endregion

    /// <summary>
    /// Joins events of two observables returning an observable which emits events
    /// when either observable emits them.
    /// </summary>
    public static IRxObservable<A> join<A, B>(
      this IRxObservable<A> o, IRxObservable<B> other
    ) where B : A => new Observable<A>(onEvent =>
      o.subscribe(NoOpDisposableTracker.instance, onEvent)
      .join(other.subscribe(NoOpDisposableTracker.instance, v => onEvent(v)))
    );

    #region #joinAll

    public static IRxObservable<A> joinAll<A>(
      this IRxObservable<A> o, IEnumerable<IRxObservable<A>> others
    ) => joinAll(o.Yield().Concat(others));

    /// <summary>
    /// Joins all events from all observables into one stream.
    /// </summary>
    public static IRxObservable<A> joinAll<A>(
      this IEnumerable<IRxObservable<A>> observables
    ) => new Observable<A>(onEvent =>
      observables.Select(aObs => aObs.subscribe(NoOpDisposableTracker.instance, onEvent))
      .ToArray().joinSubscriptions()
    );

    #endregion

    /* Joins events, but discards the values. */
    public static IRxObservable<Unit> joinDiscard<A, X>(
      this IRxObservable<A> o, IRxObservable<X> other
    ) => new Observable<Unit>(onEvent =>
      o.subscribe(NoOpDisposableTracker.instance, _ => onEvent(F.unit))
      .join(other.subscribe(NoOpDisposableTracker.instance, _ => onEvent(F.unit)))
    );

    /// <summary>
    /// Only emits an event if other event was not emmited in specified time range.
    /// </summary>
    public static IRxObservable<A> onceEvery<A>(
      this IRxObservable<A> o, Duration duration, ITimeContext timeContext = null
    ) {
      timeContext = timeContext.orDefault();
      return new Observable<A>(onEvent => {
        var lastEmit = new Duration(int.MinValue);
        return o.subscribe(
          NoOpDisposableTracker.instance,
          value => {
            var now = timeContext.passedSinceStartup;
            if (lastEmit + duration > now) return;
            lastEmit = now;
            onEvent(value);
          }
        );
      });
    }

    /// <summary>
    /// Waits until `count` events are emmited within a single `timeframe`
    /// seconds window and emits a read only linked list of
    /// (element, emission time) Tpls with emmission time.
    /// </summary>
    public static IRxObservable<ReadOnlyLinkedList<Tpl<A, float>>> withinTimeframe<A>(
      this IRxObservable<A> o, int count, Duration timeframe, TimeScale timeScale = TimeScale.Realtime
    ) =>
      new Observable<ReadOnlyLinkedList<Tpl<A, float>>>(onEvent =>
        o.map(value => F.t(value, timeScale.now()))
          .buffer(count)
          .filter(events => {
            if (events.Count != count) return false;
            var last = events.Last.Value._2;

            return events.All(t => last - t._2 <= timeframe.seconds);
          })
          .subscribe(NoOpDisposableTracker.instance, onEvent)
      );

    /// <summary>
    /// Delays each event.
    /// </summary>
    public static IRxObservable<A> delayed<A>(
      this IRxObservable<A> o, Duration delay, ITimeContext timeContext = null
    ) {
      timeContext = timeContext.orDefault();
      return new Observable<A>(onEvent => o.subscribe(
        NoOpDisposableTracker.instance,
        v => timeContext.after(delay, () => onEvent(v))
      ));
    }

    #region Changes

    /// <summary>
    /// Returns pairs of (old, new) values when they are changing.
    /// If there was no events before, old may be None.
    /// </summary>
    [PublicAPI] public static IRxObservable<Tpl<Option<A>, A>> changesOpt<A>(
      this IRxObservable<A> o, Fn<A, A, bool> areEqual = null
    ) => o.changesOpt(F.t, areEqual);

    /// <summary>
    /// Returns pairs of (old, new) values when they are changing.
    /// If there was no events before, old may be None.
    /// </summary>
    [PublicAPI] public static IRxObservable<B> changesOpt<A, B>(
      this IRxObservable<A> o, Fn<Option<A>, A, B> zipper, Fn<A, A, bool> areEqual = null
    ) {
      areEqual = areEqual ?? EqComparer<A>.Default.Equals;
      return new Observable<B>(changesBase<A, B>(
        o,
        (onEvent, lastValue, val) => {
          var valueChanged =
            lastValue.isNone || !areEqual(lastValue.__unsafeGetValue, val);
          if (valueChanged) onEvent(zipper(lastValue, val));
        }
      ));
    }

    /// <summary>
    /// Like changesOpt() but does not emit if old was None.
    /// </summary>
    public static IRxObservable<Tpl<A, A>> changes<A>(
      this IRxObservable<A> o, Fn<A, A, bool> areEqual = null
    ) {
      areEqual = areEqual ?? EqComparer<A>.Default.Equals;
      return new Observable<Tpl<A, A>>(changesBase<A, Tpl<A, A>>(
        o,
        (onEvent, lastValue, val) => {
          if (lastValue.isSome) {
            var lastVal = lastValue.__unsafeGetValue;
            if (! areEqual(lastVal, val))
              onEvent(F.t(lastVal, val));
          }
        }
      ));
    }

    /// <summary>
    /// Emits new values. Always emits first value and then emits changed values.
    /// </summary>
    public static IRxObservable<A> changedValues<A>(
      this IRxObservable<A> o, Fn<A, A, bool> areEqual = null
    ) {
      areEqual = areEqual ?? EqComparer<A>.Default.Equals;
      return new Observable<A>(changesBase<A, A>(
        o,
        (onEvent, lastValue, val) => {
          if (lastValue.isNone) onEvent(val);
          else if (! areEqual(lastValue.get, val))
            onEvent(val);
        }
      ));
    }

    static Observable<Elem>.SubscribeToSource changesBase<A, Elem>(
      IRxObservable<A> o, Act<Act<Elem>, Option<A>, A> action
    ) => onEvent => {
      var lastValue = F.none<A>();
      return o.subscribe(
        NoOpDisposableTracker.instance,
        val => {
          action(onEvent, lastValue, val);
          lastValue = F.some(val);
        }
      );
    };

    #endregion

    /// <summary>
    /// Convert this observable to reactive value with given initial value.
    /// </summary>
    public static IRxVal<A> toRxVal<A>(this IRxObservable<A> o, A initial) => new RxVal<A>(
      initial,
      setValue => o.subscribe(NoOpDisposableTracker.instance, a => setValue(a))
    );

    public static IRxVal<B> toRxVal<A, B>(this IRxObservable<A> o, B initial, Fn<A, B> mapper) => new RxVal<B>(
      initial,
      setValue => o.subscribe(NoOpDisposableTracker.instance, a => setValue(mapper(a)))
    );
  }
}