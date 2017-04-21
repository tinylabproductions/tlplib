using System;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ObservableExts {
    public static ISubscription subscribeWhileAlive<A>(
      this IObservable<A> obs, GameObject obj, Act<A> onChange
    ) {
      var onDestroySub = Subscription.empty;
      var subscription = obs.subscribe((val, sub) => {
        if (!obj) {
          onDestroySub.unsubscribe();
          sub.unsubscribe();
          return;
        }
        onChange(val);
      });
      onDestroySub = 
        obj.EnsureComponent<OnDestroyForwarder>().onDestroy
        .subscribeForOneEvent(_ => subscription.unsubscribe());
      return subscription;
    }

    public static ISubscription subscribeWhileAlive<A>(
      this IObservable<A> obs, MonoBehaviour behaviour, Act<A> onChange
    ) => obs.subscribeWhileAlive(behaviour.gameObject, onChange);
  }
}
