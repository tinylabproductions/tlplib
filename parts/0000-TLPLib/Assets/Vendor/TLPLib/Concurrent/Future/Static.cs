using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class Future {
    public struct Timeout {
      public readonly float timeoutSeconds;

      public Timeout(float timeoutSeconds) {
        this.timeoutSeconds = timeoutSeconds;
      }

      public override string ToString() { return $"{nameof(Timeout)}[in {timeoutSeconds}s]"; }
    }

    public class TimeoutException : Exception {
      public readonly Timeout timeout;

      public TimeoutException(Timeout timeout) : base($"Future timed out: {timeout}") {
        this.timeout = timeout;
      }
    }

    public static Future<A> a<A>(Act<Promise<A>> action)
      { return Future<A>.async(action); }

    public static Future<A> successful<A>(A value)
      { return Future<A>.successful(value); }

    public static Future<A> unfullfiled<A>()
      { return Future<A>.unfullfilled; }

    public static Future<A> delay<A>(float seconds, Fn<A> createValue) {
      return a<A>(p => ASync.WithDelay(seconds, () => p.complete(createValue())));
    }

    /**
     * Converts enumerable of futures into future of enumerable that is completed
     * when all futures complete.
     **/
    public static Future<A[]> sequence<A>(
      this IEnumerable<Future<A>> enumerable, string name=null
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
    (this IEnumerable<Future<A>> enumerable, Fn<A, Option<B>> predicate) {
      var futures = enumerable.ToList();
      return Future<Option<B>>.async(p => {
        var completed = 0;
        foreach (var f in futures)
          f.onComplete(a => {
            completed++;
            var res = predicate(a);
            if (res.isDefined) p.tryComplete(res);
            else if (completed == futures.Count) p.tryComplete(F.none<B>());
          });
      });
    }

    public static Future<Option<B>> firstOfSuccessful<A, B>
    (this IEnumerable<Future<Either<A, B>>> enumerable)
    { return enumerable.firstOfWhere(e => e.rightValue); }
    
    public static Future<Either<A[], B>> firstOfSuccessfulCollect<A, B>
    (this IEnumerable<Future<Either<A, B>>> enumerable) {
      var futures = enumerable.ToArray();
      return futures.firstOfSuccessful().map(opt => opt.fold(
        /* If this future is completed, then all futures are completed with lefts. */
        () => Either<A[], B>.Left(futures.Select(f => f.value.get.leftValue.get).ToArray()),
        Either<A[], B>.Right
      ));
    }

    public static Future<Unit> fromCoroutine(IEnumerator enumerator) {
      return Future<Unit>.async(p => ASync.StartCoroutine(coroutineEnum(p, enumerator)));
    }

    /* Waits at most `timeoutSeconds` for the future to complete. Completes with 
       exception produced by `onTimeout` on timeout. */
    public static Future<Either<B, A>> timeout<A, B>(
      this Future<A> future, float timeoutSeconds, Fn<B> onTimeout
    ) {
      // TODO: test me - how? Unity test runner doesn't support delays.
      var timeoutF = delay(timeoutSeconds, () => future.value.fold(
        // onTimeout() might have side effects, so we only need to execute it if 
        // there is no value in the original future once the timeout hits.
        () => onTimeout().left().r<A>(),
        v => v.right().l<B>()
      ));
      return new[] { future.map(v => v.right().l<B>()), timeoutF }.firstOf();
    }

    /* Waits at most `timeoutSeconds` for the future to complete. Completes with 
       TimeoutException<A> on timeout. */
    public static Future<Either<Timeout, A>> timeout<A>(
      this Future<A> future, float timeoutSeconds
    ) {
      return future.timeout(
        timeoutSeconds, 
        () => new Timeout(timeoutSeconds)
      );
    }

    static IEnumerator coroutineEnum(Promise<Unit> p, IEnumerator enumerator) {
      yield return ASync.StartCoroutine(enumerator);
      p.complete(Unit.instance);
    }
  }
}
