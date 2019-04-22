﻿using System;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class UnityEventExts {
    [Obsolete("Please use variant with explicit tracker passed")]
    public static ISubscription subscribe<A>(this UnityEvent<A> evt, UnityAction<A> act) {
      evt.AddListener(act);
      return new Subscription(() => evt.RemoveListener(act));
    }
    
    public static ISubscription subscribe<A>(this UnityEvent<A> evt, IDisposableTracker tracker, UnityAction<A> act) {
      evt.AddListener(act);
      var sub = new Subscription(() => evt.RemoveListener(act));
      tracker.track(sub);
      return sub;
    }

    public static IRxVal<A> toRxVal<A>(this UnityEvent<A> evt, IDisposableTracker tracker, A initial) {
      var rx = RxRef.a(initial);
      evt.subscribe(tracker,a => rx.value = a);
      return rx;
    }
  }
}