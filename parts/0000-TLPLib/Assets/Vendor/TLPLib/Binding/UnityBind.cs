using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Pools;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;
using Coroutine = com.tinylabproductions.TLPLib.Concurrent.Coroutine;

namespace com.tinylabproductions.TLPLib.binding {
  [PublicAPI] public static partial class UnityBind {
    public static ISubscription bind<A>(
      this IRxObservable<A> observable, IDisposableTracker tracker, Func<A, Coroutine> f
    ) {
      var lastCoroutine = F.none<Coroutine>();
      void stopOpt() { foreach (var c in lastCoroutine) { c.stop(); } };
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
      IDisposableTracker tracker, Func<Template, Data, ISubscription> setup,
      bool orderMatters = true
    ) where Template : Component {
      var current = new List<BindEnumerableEntry<Template>>();

      rx.subscribe(tracker,list => {
        cleanupCurrent();

        var idx = 0;
        foreach (var element in list) {
          var instance = pool.borrow();
          if (orderMatters) instance.transform.SetSiblingIndex(idx);
          var sub = setup(instance, element);
          current.Add(new BindEnumerableEntry<Template>(instance, sub));
          idx++;
        }
      });
      tracker.track(new Subscription(cleanupCurrent));
      
      void cleanupCurrent() {
        foreach (var element in current) {
          if (element.instance) pool.release(element.instance);
          element.subscription.unsubscribe();
        }
        current.Clear();
      }
    }
    
    public static GameObjectPool<Template> bindEnumerable<Template, Data>(
      string gameObjectPoolName,
      Template template, IRxObservable<IEnumerable<Data>> rx,
      IDisposableTracker tracker, Func<Template, Data, ISubscription> setup
    ) where Template : Component {
      template.gameObject.SetActive(false);
      var pool = GameObjectPool.a(GameObjectPool.Init.noReparenting(
        gameObjectPoolName,
        create: () => template.clone(parent: template.transform.parent)
      ));
      bindEnumerable(pool, rx, tracker, setup);
      return pool;
    }

    [Record] public readonly partial struct BindEnumerableEntry<Template> {
      public readonly Template instance;
      public readonly ISubscription subscription;
    }
  }
}