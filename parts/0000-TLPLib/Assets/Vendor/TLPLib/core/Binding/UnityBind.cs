using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Pools;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.collection;
using pzd.lib.concurrent;
using pzd.lib.data.dispose;
using pzd.lib.dispose;
using pzd.lib.reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.binding {
  [PublicAPI] public static partial class UnityBind {
    public static ISubscription bind<A>(
      this IRxObservable<A> observable, IDisposableTracker tracker, Func<A, ICoroutine> f
    ) {
      var lastCoroutine = F.none<ICoroutine>();
      void stopOpt() { foreach (var c in lastCoroutine) { c.stop(); } }
      var sub = observable.subscribe(
        NoOpDisposableTracker.instance,
        a => {
          stopOpt();
          lastCoroutine = f(a).some();
        }
      ).andThen(stopOpt);
      tracker.track(sub);
      return sub;
    }
    
    public static void bindEnumerable<Template, Data>(
      GameObjectPool<Template> pool,
      IRxObservable<IEnumerable<Data>> rx,
      Func<Template, Data, IDisposable> setup,
      [Implicit] IDisposableTracker tracker = default, 
      bool orderMatters = true,
      Action preUpdate = null,
      Action<List<BindEnumerableEntry<Template>>> afterUpdate = null
    ) where Template : Component {
      var current = new List<BindEnumerableEntry<Template>>();

      rx.subscribe(tracker, list => {
        cleanupCurrent();
        preUpdate?.Invoke();

        var idx = 0;
        foreach (var element in list) {
          var instance = pool.borrow();
          if (orderMatters) instance.transform.SetSiblingIndex(idx);
          var sub = setup(instance, element);
          current.Add(new BindEnumerableEntry<Template>(instance, sub));
          idx++;
        }
        afterUpdate?.Invoke(current);
      });
      // ReSharper disable once PossibleNullReferenceException
      tracker.track(new Subscription(cleanupCurrent));
      
      void cleanupCurrent() {
        foreach (var element in current) {
          if (element.instance) pool.release(element.instance);
          element.subscription.Dispose();
        }
        current.Clear();
      }
    }

    public static IRxVal<ImmutableArrayC<Result>> bindEnumerableRx<Template, Data, Result>(
      GameObjectPool<Template> pool,
      IRxObservable<IEnumerable<Data>> rx,
      Func<Template, Data, (IDisposable, Result)> setup,
      [Implicit] IDisposableTracker tracker = default, 
      bool orderMatters = true,
      Action preUpdate = null,
      Action<List<BindEnumerableEntry<Template>>> afterUpdate = null
    ) where Template : Component {
      var resultRx = RxRef.a(ImmutableArrayC<Result>.empty);
      var resultTempList = new List<Result>();
      bindEnumerable(
        pool, rx,
        orderMatters: orderMatters,
        preUpdate: () => {
          resultTempList.Clear();
          preUpdate?.Invoke();
        },
        afterUpdate: list => {
          resultRx.value = resultTempList.toImmutableArrayC();
          resultTempList.Clear();
          afterUpdate?.Invoke(list);
        },
        setup: (template, data) => {
          var (disposable, result) = setup(template, data);
          resultTempList.Add(result);
          return disposable;
        }
      );
      return resultRx;
    }
    
    public static GameObjectPool<Template> bindEnumerable<Template, Data>(
      string gameObjectPoolName,
      Template template, IRxObservable<IEnumerable<Data>> rx,
      Func<Template, Data, IDisposable> setup,
      [Implicit] IDisposableTracker tracker = default,
      Action<List<BindEnumerableEntry<Template>>> afterUpdate = null
    ) where Template : Component {
      template.gameObject.SetActive(false);
      var pool = GameObjectPool.a(GameObjectPool.Init.noReparenting(
        gameObjectPoolName,
        create: () => template.clone(parent: template.transform.parent),
        dontDestroyOnLoad: false
      ));
      bindEnumerable(pool, rx, setup, afterUpdate: afterUpdate);
      return pool;
    }

    [Record] public readonly partial struct BindEnumerableEntry<Template> {
      public readonly Template instance;
      public readonly IDisposable subscription;
    }
  }
}