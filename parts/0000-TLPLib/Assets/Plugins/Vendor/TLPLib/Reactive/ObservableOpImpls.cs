using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * Reusable implementations of common observable methods. The implementations
   * may be reused (for example: IObservable<A> -> IObservable<B> & IRxVal<A> -> IRxVal<B>)
   */
  public static class ObservableOpImpls {
    #region #map

    public static SubscribeToSource<B> map<A, B>(
      SubscribeToSource<A> subscribe, Fn<A, B> mapper
    ) =>
      obs => subscribe(new Observer<A>(val => obs.push(mapper(val))));

    #endregion

    #region #flatMap

    public static SubscribeToSource<B> flatMap<A, B>(
      IObservable<A> observable, Fn<A, IEnumerable<B>> mapper
    ) => 
      obs => observable.subscribe(val => {
        foreach (var b in mapper(val)) obs.push(b);
      });
    
    public static SubscribeToSource<B> flatMap<A, B>(
      SubscribeToSource<A> subscribe, Fn<A, IObservable<B>> mapper
    ) => 
      obs => {
        ISubscription innerSub = null;
        void innerUnsub() => innerSub?.unsubscribe();
        var observer = new Observer<A>(val => {
          innerUnsub();
          var newObs = mapper(val);
          innerSub = newObs.subscribe(obs);
        });
        var thisSub = subscribe(observer);
        return thisSub.andThen(innerUnsub);
      };

    public static SubscribeToSource<B> flatMap<A, B>(
      IObservable<A> o, Fn<A, Future<B>> mapper
    ) => 
      obs => o.subscribe(a => mapper(a).onComplete(obs.push));

    #endregion

    #region #filter

    public static SubscribeToSource<A> filter<A>(
      IObservable<A> o, Fn<A, bool> predicate
    ) =>
      obs => o.subscribe(val => { if (predicate(val)) obs.push(val); });

    #endregion

    #region #skip

    public static SubscribeToSource<A> skip<A>(IObservable<A> o, uint count) => 
      obs => {
        var skipped = 0u;
        return o.subscribe(a => {
          if (skipped < count) skipped++;
          else obs.push(a);
        });
      };

    #endregion
    
    #region #oncePerFrame

    public static SubscribeToSource<A> oncePerFrame<A>(IObservable<A> o)  =>
      obs => {
        var last = Option<A>.None;
        var mySub = o.subscribe(v => last = v.some());
        var luSub = ASync.onLateUpdate.subscribe(_ => {
          foreach (var val in last) { 
            // Clear last before pushing, because exception makes it loop forever.
            last = Option<A>.None;
            obs.push(val);
          }
        });
        return mySub.join(luSub);
      };

    #endregion

    #region #zip
    
    public static SubscribeToSource<Tpl<A, B>> zip<A, B>(
      IObservable<A> o, IObservable<B> other
    ) =>
      obs => {
        var lastSelf = F.none<A>();
        var lastOther = F.none<B>();

        void notify() {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastOther)
            obs.push(F.t(aVal, bVal));
        }

        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = other.subscribe(val => { lastOther = F.some(val); notify(); });
        return s1.join(s2);
      };

    public static SubscribeToSource<Tpl<A, B, C>> zip<A, B, C>(
      IObservable<A> o, IObservable<B> o1, IObservable<C> o2
    ) =>
      obs => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();

        void notify() {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastO1)
          foreach (var cVal in lastO2)
            obs.push(F.t(aVal, bVal, cVal));
        }

        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); });
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); });
        return s1.join(s2, s3);
      };

    public static SubscribeToSource<Tpl<A, B, C, D>> zip<A, B, C, D>(
      IObservable<A> o, IObservable<B> o1, IObservable<C> o2, IObservable<D> o3
    ) =>
      obs => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        var lastO3 = F.none<D>();

        void notify() {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastO1)
          foreach (var cVal in lastO2)
          foreach (var dVal in lastO3)
            obs.push(F.t(aVal, bVal, cVal, dVal));
        }

        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); });
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); });
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); });
        return s1.join(s2, s3, s4);
      };

    public static SubscribeToSource<Tpl<A, B, C, D, E>> zip<A, B, C, D, E>(
      IObservable<A> o, IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4
    ) =>
      obs => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        var lastO3 = F.none<D>();
        var lastO4 = F.none<E>();

        void notify() {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastO1)
          foreach (var cVal in lastO2)
          foreach (var dVal in lastO3)
          foreach (var eVal in lastO4)
            obs.push(F.t(aVal, bVal, cVal, dVal, eVal));
        }

        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); });
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); });
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); });
        var s5 = o4.subscribe(val => { lastO4 = F.some(val); notify(); });
        return s1.join(s2, s3, s4, s5);
      };

    public static SubscribeToSource<Tpl<A, A1, A2, A3, A4, A5>> zip<A, A1, A2, A3, A4, A5>(
      IObservable<A> o, IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, 
      IObservable<A4> o4, IObservable<A5> o5
    ) => 
      obs => {
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
            obs.push(F.t(aVal, a1Val, a2Val, a3Val, a4Val, a5Val));
        }

        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); });
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); });
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); });
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); });
        var s5 = o4.subscribe(val => { lastO4 = F.some(val); notify(); });
        var s6 = o5.subscribe(val => { lastO5 = F.some(val); notify(); });
        return s1.join(s2, s3, s4, s5, s6);
      };

    #endregion

    #region #discardValue

    public static SubscribeToSource<Unit> discardValue<A>(IObservable<A> o) =>
      obs => o.subscribe(_ => obs.push(F.unit));

    #endregion

    #region #collect

    public static SubscribeToSource<B> collect<A, B>(
      IObservable<A> o, Fn<A, Option<B>> collector
    ) =>
      obs => o.subscribe(
        val => { foreach (var b in collector(val)) obs.push(b); }
      );

    #endregion

    #region #buffer

    public static SubscribeToSource<C> buffer<A, C>(
      IObservable<A> o, int size, IObservableQueue<A, C> queue
    ) =>
      obs => o.subscribe(val => {
        queue.addLast(val);
        if (queue.count > size) queue.removeFirst();
        obs.push(queue.collection);
      });

    #endregion

    #region #timeBuffer

    public static SubscribeToSource<C> timeBuffer<A, C>(
      IObservable<A> o, Duration duration,
      IObservableQueue<Tpl<A, float>, C> queue,
      TimeScale timeScale
    ) {
      return obs => o.subscribe(val => {
        queue.addLast(F.t(val, timeScale.now()));
        var lastTime = queue.last._2;
        if (queue.first._2 + duration.seconds <= lastTime)
        {
          // Remove items which are too old.
          while (queue.first._2 + duration.seconds < lastTime)
            queue.removeFirst();
          obs.push(queue.collection);
        }
      });
    }

    #endregion

    #region #join

    public static SubscribeToSource<A> join<A, B>(
      IObservable<A> o, IObservable<B> other
    ) where B : A =>
      obs => 
        o.subscribe(obs.push)
        .join(other.subscribe(v => obs.push(v)));

    #endregion

    #region #joinAll

    public static SubscribeToSource<A> joinAll<A>(
      IObservable<A> o, IEnumerable<IObservable<A>> others
    ) => joinAll(o.Yield().Concat(others));

    public static SubscribeToSource<A> joinAll<A>(
      IEnumerable<IObservable<A>> observables
    ) =>
      obs =>
        observables.Select(aObs => aObs.subscribe(obs.push)).ToArray().joinSubscriptions();

    #endregion

    #region #joinDiscard

    public static SubscribeToSource<Unit> joinDiscard<A, X>(
      IObservable<A> o, IObservable<X> other
    ) =>
      obs => 
        o.subscribe(_ => obs.push(F.unit))
        .join(other.subscribe(_ => obs.push(F.unit)));

    #endregion

    #region #onceEvery

    public static SubscribeToSource<A> onceEvery<A>(
      IObservable<A> o, Duration duration, ITimeContext timeContext = null
    ) {
      timeContext = timeContext.orDefault();
      return obs => {
        var lastEmit = new Duration(int.MinValue);
        return o.subscribe(value => {
          var now = timeContext.passedSinceStartup;
          if (lastEmit + duration > now) return;
          lastEmit = now;
          obs.push(value);
        });
      };
    }

    #endregion

    #region #withinTimeframe

    public static O withinTimeframe<A, O>(
      IObservable<A> o, int count, Duration timeframe, TimeScale timeScale,
      ObserverBuilder<ReadOnlyLinkedList<Tpl<A, float>>, O> builder
    ) => builder(obs =>
      o.map(value => F.t(value, timeScale.now()))
      .buffer(count)
      .filter(events => {
        if (events.Count != count) return false;
        var last = events.Last.Value._2;

        return events.All(t => last - t._2 <= timeframe.seconds);
      })
      .subscribe(obs)
    );

    #endregion

    #region #delayed

    // TODO: test with ITimeContext
    public static SubscribeToSource<A> delayed<A>(
      IObservable<A> o, Duration delay, ITimeContext timeContext = null
    ) {
      timeContext = timeContext.orDefault();  
      return obs => o.subscribe(v => timeContext.after(delay, () => obs.push(v)));
    }

    #endregion

    #region #changes

    static SubscribeToSource<Elem> changesBase<A, Elem>(
      IObservable<A> o, Act<IObserver<Elem>, Option<A>, A> action
    ) => obs => {
      var lastValue = F.none<A>();
      return o.subscribe(val => {
        action(obs, lastValue, val);
        lastValue = F.some(val);
      });
    };

    public static SubscribeToSource<Tpl<Option<A>, A>> changesOpt<A>(
      IObservable<A> o, Fn<A, A, bool> areEqual
    ) => 
      changesBase<A, Tpl<Option<A>, A>>(o, (obs, lastValue, val) => {
        var valueChanged = lastValue.fold(
          () => true,
          lastVal => !areEqual(lastVal, val)
        );
        if (valueChanged) obs.push(F.t(lastValue, val));
      });

    public static SubscribeToSource<Tpl<A, A>> changes<A>(
      IObservable<A> o, Fn<A, A, bool> areEqual
    ) =>
      changesBase<A, Tpl<A, A>>(o, (obs, lastValue, val) => {
        foreach (var lastVal in lastValue)
          if (!areEqual(lastVal, val))
            obs.push(F.t(lastVal, val));
      });

    public static SubscribeToSource<A> changedValues<A>(
      IObservable<A> o, Fn<A, A, bool> areEqual
    ) => changesBase<A, A>(o, (obs, lastValue, val) => {
      if (lastValue.isEmpty) obs.push(val);
      else if (! areEqual(lastValue.get, val))
        obs.push(val);
    });

    #endregion

  }
}