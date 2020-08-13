using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [PublicAPI] public static class FutureU {
    public static Future<A> delayFrames<A>(int framesToSkip, Func<A> createValue) =>
      Future.a<A>(p => ASync.AfterXFrames(framesToSkip, () => p.complete(createValue())));

    public static Future<A> delayFrames<A>(int framesToSkip, A value) =>
      Future.a<A>(p => ASync.AfterXFrames(framesToSkip, () => p.complete(value)));
      
    public static Future<Unit> fromCoroutine(IEnumerator enumerator) =>
      Future.fromCoroutine(ASync.StartCoroutine(enumerator));

    public static Future<A> fromBusyLoop<A>(
      Func<Option<A>> checker, YieldInstruction delay=null
    ) => Future.async<A>(p => ASync.StartCoroutine(busyLoopEnum(delay, p, checker)));

    /// <summary>Complete when checker returns true</summary>
    public static Future<Unit> fromBusyLoop(
      Func<bool> checker, YieldInstruction delay=null
    ) => Future.async<Unit>(p => ASync.StartCoroutine(busyLoopEnum(delay, p, checker)));

    /** Measures how much time has passed from call to timed to future completion. **/
    public static Future<Tpl<A, Duration>> timed<A>(this Future<A> future) {
      var startTime = Time.realtimeSinceStartup;
      return future.map(a => {
        var time = Time.realtimeSinceStartup - startTime;
        return F.t(a, Duration.fromSeconds(time));
      });
    }

    static IEnumerator busyLoopEnum<A>(YieldInstruction delay, Promise<A> p, Func<Option<A>> checker) {
      var valOpt = checker();
      while (valOpt.isNone) {
        yield return delay;
        valOpt = checker();
      }
      p.complete(valOpt.get);
    }

    static IEnumerator busyLoopEnum(YieldInstruction delay, Promise<Unit> p, Func<bool> checker) {
      while (!checker()) {
        yield return delay;
      }
      p.complete(F.unit);
    }
  }
}