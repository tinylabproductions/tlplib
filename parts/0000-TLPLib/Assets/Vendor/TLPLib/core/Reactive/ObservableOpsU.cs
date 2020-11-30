using System;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Components.dispose;
using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;
using pzd.lib.data;
using pzd.lib.dispose;
using pzd.lib.reactive;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  [PublicAPI] public static class ObservableOpsU {
    public static ISubscription subscribe<A>(
      this IRxObservable<A> observable, GameObject tracker, Action<A> onEvent,
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

    // If several events are emitted per same frame, only emit last one in late update.
    // TODO: test, but how? Can't do async tests in unity.
    public static IRxObservable<A> oncePerFrame<A>(this IRxObservable<A> o) =>
      new Observable<A>(onEvent => {
        var last = Option<A>.None;
        var mySub = o.subscribe(NoOpDisposableTracker.instance, v => last = v.some());
        var luSub = ASync.onLateUpdate.subscribe(NoOpDisposableTracker.instance, _ => {
          foreach (var val in last) {
            // Clear last before pushing, because exception makes it loop forever.
            last = None._;
            onEvent(val);
          }
        });
        return mySub.join(luSub);
      });
    
    /// <summary>
    /// Only emit an event if it's the first event in this frame.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="frameNoRx">Reference to last frame number. Can be shared between several observables.</param>
    public static IRxObservable<A> oncePerFrameShared<A>(this IRxObservable<A> o, Ref<int> frameNoRx) =>
      new Observable<A>(onEvent => o.subscribe(NoOpDisposableTracker.instance, a => {
        var frameNo = Time.frameCount;
        if (frameNoRx.value != frameNo) {
          frameNoRx.value = frameNo;
          onEvent(a);
        }
      }));
  }
}