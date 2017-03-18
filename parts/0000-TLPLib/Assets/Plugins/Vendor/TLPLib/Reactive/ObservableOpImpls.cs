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

    public static SubscribeFn<B> map<A, B>(
      IObservable<A> observable, Fn<A, B> mapper
    ) =>
      obs => observable.subscribe(val => obs.push(mapper(val)), obs.finish);

    #endregion

    #region #flatMap

    public static SubscribeFn<B> flatMap<A, B>(
      IObservable<A> observable, Fn<A, IEnumerable<B>> mapper
    ) => 
      obs => observable.subscribe(val => {
        foreach (var b in mapper(val)) obs.push(b);
      }, obs.finish);

    public static SubscribeFn<B> flatMap<A, B>(
      IObservable<A> o, Fn<A, IObservable<B>> mapper, Act<IObservable<B>> newObsGot = null
    ) => flatMap<A, B, IObservable<B>>(o, mapper, newObsGot);

    public static SubscribeFn<B> flatMap<A, B, Obs>(
      IObservable<A> o, Fn<A, Obs> mapper, Act<Obs> newObsGot = null
    ) where Obs : IObservable<B> => 
      obs => {
        ISubscription innerSub = null;
        Action innerUnsub = () => innerSub?.unsubscribe();
        var thisSub = o.subscribe(
          val => {
            innerUnsub();
            var newObs = mapper(val);
            newObsGot?.Invoke(newObs);
            innerSub = newObs.subscribe(obs);
          },
          () => {
            innerUnsub();
            obs.finish();
          }
        );
        return thisSub.andThen(innerUnsub);
      };

    public static SubscribeFn<B> flatMap<A, B>(
      IObservable<A> o, Fn<A, Future<B>> mapper
    ) => 
      obs => {
        var sourceFinished = false;
        return o.subscribe(
          a => mapper(a).onComplete(b => {
            if (!sourceFinished) obs.push(b);
          }),
          () => {
            sourceFinished = true;
            obs.finish();
          }
        );
      };

    #endregion

    #region #filter

    public static SubscribeFn<A> filter<A>(
      IObservable<A> o, Fn<A, bool> predicate
    ) =>
      obs => o.subscribe(
        val => { if (predicate(val)) obs.push(val); },
        obs.finish
      );

    #endregion

    #region #skip

    public static SubscribeFn<A> skip<A>(IObservable<A> o, uint count) => 
      obs => {
        var skipped = 0u;
        return o.subscribe(
          a => {
            if (skipped < count) skipped++;
            else obs.push(a);
          },
          obs.finish
        );
      };

    #endregion
    
    #region #oncePerFrame

    public static SubscribeFn<A> oncePerFrame<A>(IObservable<A> o)  =>
      obs => {
        var last = Option<A>.None;
        var mySub = o.subscribe(v => last = v.some(), obs.finish);
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
    
    public static SubscribeFn<Tpl<A, B>> zip<A, B>(
      IObservable<A> o, IObservable<B> other
    ) =>
      obs => multipleFinishes(obs, 2, checkFinish => {
        var lastSelf = F.none<A>();
        var lastOther = F.none<B>();
        Action notify = () => {
          foreach (var aVal in lastSelf)
            foreach (var bVal in lastOther)
              obs.push(F.t(aVal, bVal));
        };
        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = other.subscribe(val => { lastOther = F.some(val); notify(); }, checkFinish);
        return s1.join(s2);
      });

    public static SubscribeFn<Tpl<A, B, C>> zip<A, B, C>(
      IObservable<A> o, IObservable<B> o1, IObservable<C> o2
    ) =>
      obs => multipleFinishes(obs, 3, checkFinish => {
        var lastSelf = F.none<A>();
        var lastO1 = F.none<B>();
        var lastO2 = F.none<C>();
        Action notify = () => {
          foreach (var aVal in lastSelf)
          foreach (var bVal in lastO1)
          foreach (var cVal in lastO2)
            obs.push(F.t(aVal, bVal, cVal));
        };
        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3);
      });

    public static SubscribeFn<Tpl<A, B, C, D>> zip<A, B, C, D>(
      IObservable<A> o, IObservable<B> o1, IObservable<C> o2, IObservable<D> o3
    ) =>
      obs => multipleFinishes(obs, 4, checkFinish => {
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
        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3, s4);
      });

    public static SubscribeFn<Tpl<A, B, C, D, E>> zip<A, B, C, D, E>(
      IObservable<A> o, IObservable<B> o1, IObservable<C> o2, IObservable<D> o3, IObservable<E> o4
    ) =>
      obs => multipleFinishes(obs, 5, checkFinish => {
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
        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); }, checkFinish);
        var s5 = o4.subscribe(val => { lastO4 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3, s4, s5);
      });

    public static SubscribeFn<Tpl<A, A1, A2, A3, A4, A5>> zip<A, A1, A2, A3, A4, A5>(
      IObservable<A> o, IObservable<A1> o1, IObservable<A2> o2, IObservable<A3> o3, 
      IObservable<A4> o4, IObservable<A5> o5
    ) => 
      obs => multipleFinishes(obs, 6, checkFinish => {
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
        var s1 = o.subscribe(val => { lastSelf = F.some(val); notify(); }, checkFinish);
        var s2 = o1.subscribe(val => { lastO1 = F.some(val); notify(); }, checkFinish);
        var s3 = o2.subscribe(val => { lastO2 = F.some(val); notify(); }, checkFinish);
        var s4 = o3.subscribe(val => { lastO3 = F.some(val); notify(); }, checkFinish);
        var s5 = o4.subscribe(val => { lastO4 = F.some(val); notify(); }, checkFinish);
        var s6 = o5.subscribe(val => { lastO5 = F.some(val); notify(); }, checkFinish);
        return s1.join(s2, s3, s4, s5, s6);
      });

    #endregion

    #region #discardValue

    public static SubscribeFn<Unit> discardValue<A>(IObservable<A> o) =>
      obs => o.subscribe(_ => obs.push(F.unit), obs.finish);

    #endregion

    #region #collect

    public static SubscribeFn<B> collect<A, B>(
      IObservable<A> o, Fn<A, Option<B>> collector
    ) =>
      obs => o.subscribe(
        val => { foreach (var b in collector(val)) obs.push(b); },
        obs.finish
      );

    #endregion

    #region #buffer

    public static SubscribeFn<C> buffer<A, C>(
      IObservable<A> o, int size, IObservableQueue<A, C> queue
    ) =>
      obs => o.subscribe(
        val => {
          queue.addLast(val);
          if (queue.count > size) queue.removeFirst();
          obs.push(queue.collection);
        },
        obs.finish
      );

    #endregion

    #region #timeBuffer

    public static SubscribeFn<C> timeBuffer<A, C>(
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

    public static SubscribeFn<A> join<A, B>(
      IObservable<A> o, IObservable<B> other
    ) where B : A =>
      obs => multipleFinishes(obs, 2, checkFinished =>
        o.subscribe(
          obs.push,
          checkFinished
        ).join(other.subscribe(
          v => obs.push(v),
          checkFinished
        ))
      );

    #endregion

    #region #joinAll

    public static SubscribeFn<A> joinAll<A>(
      IObservable<A> o, IEnumerable<IObservable<A>> others, int othersCount
    ) => joinAll(o.Yield().Concat(others), 1 + othersCount);

    public static SubscribeFn<A> joinAll<A>(
      IEnumerable<IObservable<A>> observables, int count
    ) =>
      obs => multipleFinishes(obs, count, checkFinished =>
        observables.Select(aObs =>
          aObs.subscribe(obs.push, checkFinished)
        ).ToArray().joinSubscriptions()
      );

    #endregion

    #region #joinDiscard

    public static SubscribeFn<Unit> joinDiscard<A, X>(
      IObservable<A> o, IObservable<X> other
    ) =>
      obs => multipleFinishes(obs, 2, checkFinished =>
        o.subscribe(
          _ => obs.push(F.unit),
          checkFinished
        ).join(other.subscribe(
          v => obs.push(F.unit),
          checkFinished
        ))
      );

    #endregion

    #region #onceEvery

    public static SubscribeFn<A> onceEvery<A>(
      IObservable<A> o, Duration duration, TimeScale timeScale
    ) => 
      obs => {
        var lastEmit = float.NegativeInfinity;
        return o.subscribe(
          value => {
            var now = timeScale.now();
            if (lastEmit + duration.seconds > now) return;
            lastEmit = now;
            obs.push(value);
          },
          obs.finish
        );
      };

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
    public static SubscribeFn<A> delayed<A>(IObservable<A> o, Duration delay) => 
      obs => o.subscribe(
        v => ASync.WithDelay(delay, () => obs.push(v)),
        () => ASync.WithDelay(delay, obs.finish)
      );

    #endregion

    #region #changes

    static SubscribeFn<Elem> changesBase<A, Elem>(
      IObservable<A> o, Act<IObserver<Elem>, Option<A>, A> action
    ) => obs => {
      var lastValue = F.none<A>();
      return o.subscribe(val => {
        action(obs, lastValue, val);
        lastValue = F.some(val);
      }, obs.finish);
    };

    public static SubscribeFn<Tpl<Option<A>, A>> changesOpt<A>(
      IObservable<A> o, Fn<A, A, bool> areEqual
    ) => 
      changesBase<A, Tpl<Option<A>, A>>(o, (obs, lastValue, val) => {
        var valueChanged = lastValue.fold(
          () => true,
          lastVal => !areEqual(lastVal, val)
        );
        if (valueChanged) obs.push(F.t(lastValue, val));
      });

    public static SubscribeFn<Tpl<A, A>> changes<A>(
      IObservable<A> o, Fn<A, A, bool> areEqual
    ) =>
      changesBase<A, Tpl<A, A>>(o, (obs, lastValue, val) => {
        foreach (var lastVal in lastValue)
          if (!areEqual(lastVal, val))
            obs.push(F.t(lastVal, val));
      });

    public static SubscribeFn<A> changedValues<A>(
      IObservable<A> o, Fn<A, A, bool> areEqual
    ) => changesBase<A, A>(o, (obs, lastValue, val) => {
      if (lastValue.isEmpty) obs.push(val);
      else if (! areEqual(lastValue.get, val))
        obs.push(val);
    });

    #endregion

    #region Helpers

    public static Ret multipleFinishes<B, Ret>(IObserver<B> obs, int count, Fn<Action, Ret> f) {
      var finished = 0;
      Action checkFinish = () => {
        finished++;
        if (finished == count) obs.finish();
      };
      return f(checkFinish);
    }

    #endregion
  }
}