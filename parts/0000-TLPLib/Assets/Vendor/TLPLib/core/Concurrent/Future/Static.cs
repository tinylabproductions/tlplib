using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Functional.higher_kinds;
using pzd.lib.concurrent;
using pzd.lib.functional;
using pzd.lib.functional.higher_kinds;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class Future {
    // Witness for higher kinded types simulation
    public struct W {}
    
    public static Future<A> narrowK<A>(this HigherKind<W, A> hkt) => (Future<A>) hkt;
    
    public static Future<A> a<A>(Action<Promise<A>> action) => Future<A>.async(action);
    public static Future<A> a<A>(IHeapFuture<A> future) => new Future<A>(future);
    public static Future<A> async<A>(out Promise<A> promise) => Future<A>.async(out promise);

    public static Future<A> successful<A>(A value) => Future<A>.successful(value);
    public static Future<A> unfulfilled<A>() => Future<A>.unfulfilled;

    public static Future<A> delay<A>(Duration duration, Func<A> createValue, ITimeContext tc) =>
      a<A>(p => tc.after(duration, () => p.complete(createValue())));

    public static Future<A> delay<A>(Duration duration, A value, ITimeContext tc) =>
      a<A>(p => tc.after(duration, () => p.complete(value)));

    public static Future<A> delayFrames<A>(int framesToSkip, Func<A> createValue) =>
      a<A>(p => ASync.AfterXFrames(framesToSkip, () => p.complete(createValue())));

    public static Future<A> delayFrames<A>(int framesToSkip, A value) =>
      a<A>(p => ASync.AfterXFrames(framesToSkip, () => p.complete(value)));

    /**
     * Converts enumerable of futures into future of enumerable that is completed
     * when all futures complete.
     **/
    public static Future<A[]> sequence<A>(
      this IEnumerable<Future<A>> enumerable
    ) {
      var completed = 0u;
      var sourceFutures = enumerable.ToList();
      var results = new A[sourceFutures.Count];
      return a<A[]>(p => {
        for (var idx = 0; idx < sourceFutures.Count; idx++) {
          var f = sourceFutures[idx];
          var fixedIdx = idx;
          f.onComplete(value => {
            results[fixedIdx] = value;
            completed++;
            if (completed == results.Length) p.tryComplete(results);
          });
        }
      });
    }

    /**
     * Returns result from the first future that completes.
     **/
    public static Future<A> firstOf<A>(this IEnumerable<Future<A>> enumerable) {
      return Future<A>.async(p => {
        foreach (var f in enumerable) f.onComplete(v => p.tryComplete(v));
      });
    }

    /**
     * Returns result from the first future that satisfies the predicate as a Some.
     * If all futures do not satisfy the predicate returns None.
     **/
    public static Future<Option<B>> firstOfWhere<A, B>
    (this IEnumerable<Future<A>> enumerable, Func<A, Option<B>> predicate) {
      var futures = enumerable.ToList();
      return Future<Option<B>>.async(p => {
        var completed = 0;
        foreach (var f in futures)
          f.onComplete(a => {
            completed++;
            var res = predicate(a);
            if (res.isSome) p.tryComplete(res);
            else if (completed == futures.Count) p.tryComplete(F.none<B>());
          });
      });
    }

    public static Future<Option<B>> firstOfSuccessful<A, B>
    (this IEnumerable<Future<Either<A, B>>> enumerable)
    { return enumerable.firstOfWhere(e => e.rightValue); }

    public static Future<Either<A[], B>> firstOfSuccessfulCollect<A, B>
    (this IEnumerable<Future<Either<A, B>>> enumerable) {
      return enumerable.firstOfSuccessfulCollect(_ => _.ToArray());
    }

    public static Future<Either<Collection, B>> firstOfSuccessfulCollect<A, B, Collection>(
      this IEnumerable<Future<Either<A, B>>> enumerable,
      Func<IEnumerable<A>, Collection> collector
    ) {
      var futures = enumerable.ToArray();
      return futures.firstOfSuccessful().map(opt => opt.fold(
        /* If this future is completed, then all futures are completed with lefts. */
        () => Either<Collection, B>.Left(
          collector(futures.Select(f => f.value.__unsafeGet.leftValue.get))
        ),
        Either<Collection, B>.Right
      ));
    }

    public static Future<Unit> fromCoroutine(IEnumerator enumerator) =>
      fromCoroutine(ASync.StartCoroutine(enumerator));

    public static Future<Unit> fromCoroutine(Coroutine coroutine) =>
      Future<Unit>.async(p => {
        if (coroutine.finished) p.complete(F.unit);
        else {
          void onComplete() {
            p.complete(F.unit);
            coroutine.onFinish -= onComplete;
          }

          coroutine.onFinish += onComplete;
        }
      });

    public static Future<A> fromBusyLoop<A>(
      Func<Option<A>> checker, YieldInstruction delay=null
    ) => Future<A>.async(p => ASync.StartCoroutine(busyLoopEnum(delay, p, checker)));

    /// <summary>Complete when checker returns true</summary>
    public static Future<Unit> fromBusyLoop(
      Func<bool> checker, YieldInstruction delay=null
    ) => Future<Unit>.async(p => ASync.StartCoroutine(busyLoopEnum(delay, p, checker)));

    /* Waits at most `timeout` for the future to complete. Completes with
       exception produced by `onTimeout` on timeout. */
    public static Future<Either<B, A>> timeout<A, B>(
      this Future<A> future, Duration timeout, Func<B> onTimeout, ITimeContext tc
    ) {
      var timeoutF = delay(timeout, () => future.value.fold<Either<B, A>>(
        // onTimeout() might have side effects, so we only need to execute it if
        // there is no value in the original future once the timeout hits.
        () => onTimeout(),
        v => v
      ), tc);
      return new[] { future.map<Either<B, A>>(v => v), timeoutF }.firstOf();
    }

    /// <summary>Waits at most `timeout` for the future to complete.</summary>
    public static Future<Either<Duration, A>> timeout<A>(
      this Future<A> future, Duration timeout, ITimeContext tc
    ) => future.timeout(timeout, () => timeout, tc);

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
