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
  }
}