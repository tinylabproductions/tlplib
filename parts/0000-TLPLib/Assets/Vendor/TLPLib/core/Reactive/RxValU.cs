using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  [PublicAPI] public static class RxValU {
    public static IRxVal<Option<A>> fromBusyLoop<A>(Func<Option<A>> func, YieldInstruction delay=null) {
      var rx = RxRef.a(Option<A>.None);
      ASync.StartCoroutine(coroutine());
      return rx;

      IEnumerator coroutine() {
        while (true) {
          var maybeValue = func();
          if (maybeValue.valueOut(out var value)) {
            rx.value = Some.a(value);
            yield break;
          }
          else {
            yield return delay;
          }
        }
      }
    }

    /// <summary>
    /// Provides a <see cref="RxVal"/> which is only reactive when in Unity Editor.
    ///
    /// Useful when you want to make something easily inspectable from Unity Editor, so that when you change the values
    /// in Unity inspector you would see things refreshed in the game.
    ///
    /// While this is useful in the Editor, we don't want such overhead in runtime and thus simply return a wrapped
    /// value.  
    /// </summary>
    public static IRxVal<A> refreshedInEditor<A>(Func<A> calculate) => 
      Application.isEditor 
        ? ObservableU.everyFrame.map(_ => calculate()).toRxVal(calculate()) 
        : RxVal.a(calculate());
  }
}