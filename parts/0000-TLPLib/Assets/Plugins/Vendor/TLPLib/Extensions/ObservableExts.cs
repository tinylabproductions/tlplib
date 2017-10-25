using System;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ObservableExts {
    public static ISubscription subscribeWhileAlive<A>(
      this IObservable<A> obs, GameObject obj, Act<A, ISubscription> onChange
    ) {
      var onDestroySub = Subscription.empty;
      var subscription = obs.subscribe((val, sub) => {
        if (!obj) {
          onDestroySub.unsubscribe();
          sub.unsubscribe();
          return;
        }
        onChange(val, sub);
      });
      onDestroySub = 
        obj.EnsureComponent<OnDestroyForwarder>().onEvent
        .subscribeForOneEvent(_ => subscription.unsubscribe());
      return subscription;
    }

    // If you do Destroy(behaviour) gameObject with subscription is still alive
    //public static ISubscription subscribeWhileAlive<A>(
    //  this IObservable<A> obs, MonoBehaviour behaviour, Act<A, ISubscription> onChange
    //) => obs.subscribeWhileAlive(behaviour.gameObject, onChange);

    public static ISubscription subscribeWhileAlive<A>(
      this IObservable<A> obs, GameObject obj, Act<A> onChange
    ) => subscribeWhileAlive(obs, obj, (val, _) => onChange(val));
  }
}
