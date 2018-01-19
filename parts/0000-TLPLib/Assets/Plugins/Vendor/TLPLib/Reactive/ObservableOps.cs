using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.dispose;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class ObservableOps {
    #region #subscribe

    public static ISubscription subscribe<A>(
      this IObservable<A> observable, 
      IDisposableTracker tracker,
      Act<A, ISubscription> onChange
    ) {
      ISubscription subscription = null;
      // ReSharper disable once AccessToModifiedClosure
      subscription = observable.subscribe(tracker, a => onChange(a, subscription));
      return subscription;
    }

    public static ISubscription subscribe<A>(
      this IObservable<A> observable, GameObject tracker, Act<A> onChange
    ) => observable.subscribe(tracker.asDisposableTracker(), onChange);

    public static ISubscription subscribeForOneEvent<A>(
      this IObservable<A> observable, 
      IDisposableTracker tracker,
      Act<A> onEvent
    ) => observable.subscribe(tracker, (a, sub) => {
      sub.unsubscribe();
      onEvent(a);
    });

    #endregion

    /// <summary>
    /// Return self as IObservable.
    /// </summary>
    public static IObservable<A> asObservable<A>(this IObservable<A> observable) => observable;

    /** Maps events coming from this observable. **/
    public static IObservable<B> map<A, B>(
      this IObservable<A> o, Fn<A, B> mapper
    ) => new Observable<B>(onEvent => 
      o.subscribe(NoOpDisposableTracker.instance, val => onEvent(mapper(val)))
    );

    #region #flatMap

    /// <summary>
    /// Maps events coming from this observable and emits all events contained
    /// in returned enumerable.
    /// </summary>
    public static IObservable<B> flatMap<A, B>(
      this IObservable<A> o, Fn<A, IEnumerable<B>> mapper
    ) => new Observable<B>(onEvent => o.subscribe(NoOpDisposableTracker.instance, val => {
      foreach (var b in mapper(val)) onEvent(b);
    }));

    /// <summary>
    /// Maps events coming from this observable and emits all events that are emitted
    /// by returned observable.
    /// </summary>
    public static IObservable<B> flatMap<A, B>(
      this IObservable<A> o, Fn<A, IObservable<B>> mapper
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
    public static IObservable<B> flatMap<A, B>(
      this IObservable<A> o, Fn<A, Future<B>> mapper
    ) => new Observable<B>(onEvent => 
      o.subscribe(NoOpDisposableTracker.instance, a => mapper(a).onComplete(onEvent))
    );

    /// <summary>
    /// Wait until future completes and start emmiting events from the created
    /// observable then. 
    /// </summary>
    public static IObservable<B> flatMap<A, B>(
      this Future<A> future, Fn<A, IObservable<B>> mapper
    ) {
      var s = new Subject<B>();
      future.onComplete(a => mapper(a).subscribe(NoOpDisposableTracker.instance, s.push));
      return s;
    }

    #endregion

    /** Only emits events that pass the predicate. **/
    public static IObservable<A> filter<A>(
      this IObservable<A> o, Fn<A, bool> predicate
    ) => new Observable<A>(onEvent => 
      o.subscribe(NoOpDisposableTracker.instance, val => { if (predicate(val)) onEvent(val); })
    );

    /** Emits first value to the future and unsubscribes. **/
    public static Future<A> toFuture<A>(this IObservable<A> o) =>
      Future<A>.async((p, f) => {
        var subscription = o.subscribe(NoOpDisposableTracker.instance, p.complete);
        f.onComplete(_ => subscription.unsubscribe());
      });

    // Skips `count` values from the stream.
    public static IObservable<A> skip<A>(this IObservable<A> o, uint count) =>
      new Observable<A>(onEvent => {
        var skipped = 0u;
        return o.subscribe(NoOpDisposableTracker.instance, a => {
          if (skipped < count) skipped++;
          else onEvent(a);
        });
      });

    // If several events are emitted per same frame, only emit last one in late update.
    // TODO: test, but how? Can't do async tests in unity.
    public static IObservable<A> oncePerFrame<A>(this IObservable<A> o) => 
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

    public static IObservable<R> zip<A1, A2, R>(
      this IObservable<A1> o, IObservable<A2> other, Fn<A1, A2, R> zipper
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
    public static IObservable<Tpl<A1, A2>> zip<A1, A2>(
      this IObservable<A1> o1, IObservable<A2> o2
    ) => o1.zip(o2, F.t);

    public static IObservable<R> zip<A1, A2, A3, R>(
      this IObservable<A1> o, IObservable<A2> o1, IObservable<A3> o2, Fn<A1, A2, A3, R> zipper
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
    public static IObservable<Tpl<A1, A2, A3>> zip<A1, A2, A3>(
      this IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3
    ) => o1.zip(o2, o3, F.t);

    public static IObservable<R> zip<A1, A2, A3, A4, R>(
      this IObservable<A1> o, IObservable<A2> o1, IObservable<A3> o2, IObservable<A4> o3,
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
    public static IObservable<Tpl<A1, A2, A3, A4>> zip<A1, A2, A3, A4>(
      this IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, IObservable<A4> o4
    ) => o1.zip(o2, o3, o4, F.t);

    public static IObservable<R> zip<A1, A2, A3, A4, A5, R>(
      this IObservable<A1> o, IObservable<A2> o1, IObservable<A3> o2, IObservable<A4> o3, IObservable<A5> o4,
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
    public static IObservable<Tpl<A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      this IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, IObservable<A4> o4, IObservable<A5> o5
    ) => o1.zip(o2, o3, o4, o5, F.t);

    public static IObservable<R> zip<A, A1, A2, A3, A4, A5, R>(
      this IObservable<A> o, IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, 
      IObservable<A4> o4, IObservable<A5> o5, Fn<A, A1, A2, A3, A4, A5, R> zipper
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
    public static IObservable<Tpl<A1, A2, A3, A4, A5, A6>> zip<A1, A2, A3, A4, A5, A6>(
      this IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, IObservable<A4> o4, IObservable<A5> o5,
      IObservable<A6> o6
    ) => o1.zip(o2, o3, o4, o5, o6, F.t);

    #endregion

    /// <summary>
    /// Discards values that this observable emits, turning it into event
    /// source that does not carry data with it.
    /// </summary>
    public static IObservable<Unit> discardValue<A>(this IObservable<A> o) =>
      new Observable<Unit>(onEvent => 
        o.subscribe(NoOpDisposableTracker.instance, _ => onEvent(F.unit))
      );

    /// <summary>
    /// Only emits events that return some.
    /// </summary>
    public static IObservable<B> collect<A, B>(
      this IObservable<A> o, Fn<A, Option<B>> collector
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
    public static IObservable<ReadOnlyLinkedList<A>> buffer<A>(
      this IObservable<A> o, int size
    ) => o.buffer(size, new ObservableReadOnlyLinkedListQueue<A>());

    public static IObservable<C> buffer<A, C>(
      this IObservable<A> o, int size, IObservableQueue<A, C> queue
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
    public static IObservable<ReadOnlyLinkedList<Tpl<A, float>>> timeBuffer<A>(
      this IObservable<A> o, Duration duration, TimeScale timeScale = TimeScale.Realtime
    ) => o.timeBuffer(duration, new ObservableReadOnlyLinkedListQueue<Tpl<A, float>>(), timeScale);

    public static IObservable<C> timeBuffer<A, C>(
      this IObservable<A> o, Duration duration, IObservableQueue<Tpl<A, float>, C> queue,
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
    public static IObservable<A> join<A, B>(
      this IObservable<A> o, IObservable<B> other
    ) where B : A => new Observable<A>(onEvent => 
      o.subscribe(NoOpDisposableTracker.instance, onEvent)
      .join(other.subscribe(NoOpDisposableTracker.instance, v => onEvent(v)))
    );

    #region #joinAll

    public static IObservable<A> joinAll<A>(
      this IObservable<A> o, IEnumerable<IObservable<A>> others
    ) => joinAll(o.Yield().Concat(others));

    /// <summary>
    /// Joins all events from all observables into one stream.
    /// </summary>
    public static IObservable<A> joinAll<A>(
      this IEnumerable<IObservable<A>> observables
    ) => new Observable<A>(onEvent =>
      observables.Select(aObs => aObs.subscribe(NoOpDisposableTracker.instance, onEvent))
      .ToArray().joinSubscriptions()
    );

    #endregion

    /* Joins events, but discards the values. */
    public static IObservable<Unit> joinDiscard<A, X>(
      this IObservable<A> o, IObservable<X> other
    ) => new Observable<Unit>(onEvent => 
      o.subscribe(NoOpDisposableTracker.instance, _ => onEvent(F.unit))
      .join(other.subscribe(NoOpDisposableTracker.instance, _ => onEvent(F.unit)))
    );

    /// <summary>
    /// Only emits an event if other event was not emmited in specified time range.
    /// </summary>
    public static IObservable<A> onceEvery<A>(
      this IObservable<A> o, Duration duration, ITimeContext timeContext = null
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
    public static IObservable<ReadOnlyLinkedList<Tpl<A, float>>> withinTimeframe<A>(
      this IObservable<A> o, int count, Duration timeframe, TimeScale timeScale = TimeScale.Realtime
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
    public static IObservable<A> delayed<A>(
      this IObservable<A> o, Duration delay, ITimeContext timeContext = null
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
    public static IObservable<Tpl<Option<A>, A>> changesOpt<A>(
      this IObservable<A> o, Fn<A, A, bool> areEqual = null
    ) {
      areEqual = areEqual ?? EqComparer<A>.Default.Equals;
      return new Observable<Tpl<Option<A>, A>>(changesBase<A, Tpl<Option<A>, A>>(
        o, 
        (onEvent, lastValue, val) => {
          var valueChanged =
            lastValue.isNone || !areEqual(lastValue.__unsafeGetValue, val);
          if (valueChanged) onEvent(F.t(lastValue, val));
        }
      ));
    }

    /// <summary>
    /// Like changesOpt() but does not emit if old was None.
    /// </summary>
    public static IObservable<Tpl<A, A>> changes<A>(
      this IObservable<A> o, Fn<A, A, bool> areEqual = null
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
    public static IObservable<A> changedValues<A>(
      this IObservable<A> o, Fn<A, A, bool> areEqual = null
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
      IObservable<A> o, Act<Act<Elem>, Option<A>, A> action
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
    public static IRxVal<A> toRxVal<A>(this IObservable<A> o, A initial) => new RxVal<A>(
      initial,
      setValue => o.subscribe(NoOpDisposableTracker.instance, a => setValue(a))
    );
  }
}