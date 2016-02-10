using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class FutureExts {
    public static Future<B> map<A, B>(this Future<A> future, Fn<A, B> mapper, string name=null) {
      var p = new FutureImpl<B>(name ?? $"{future.name}.map");
      future.onComplete(t => t.voidFold(
        v => {
          try { p.completeSuccess(mapper(v)); }
          catch (Exception e) { p.completeError(e); }
        },
        p.completeError
      ));
      return p;
    }

    public static Future<B> flatMap<A, B>(
      this Future<A> future, Fn<A, Future<B>> mapper, string name=null
    ) {
      var p = new FutureImpl<B>(name ?? $"{future.name}.flatMap");
      future.onComplete(t => t.voidFold(
        v => {
          try { mapper(v).onComplete(p.complete); }
          catch (Exception e) { p.completeError(e); }
        },
        p.completeError
      ));
      return p;
    }

    /* Given future and a recovery function return a new future, which 
     * calls recovery function on exception in the original future and 
     * completes the new function with value on Some or exception on None. */
    public static Future<A> recover<A>(
      this Future<A> future, Fn<Exception, Option<A>> recoverFn, string name=null
    ) {
      var f = new FutureImpl<A>(name ?? $"{future.name}.recover");
      future.onComplete(t => t.voidFold(
        f.completeSuccess, e => recoverFn(e).voidFold(
          () => f.completeError(e), f.completeSuccess
        ))
      );
      return f;
    }

    /* Given future and a recovery function return a new future, which 
     * calls recovery function on exception in the original future and 
     * completes the new function with value on Some or exception on None. */
    public static Future<A> recover<A>(
      this Future<A> future, Fn<Exception, Option<Future<A>>> recoverFn, 
      string name=null
    ) {
      var f = new FutureImpl<A>(name ?? $"{future.name}.recover(future)");
      future.onComplete(t => t.voidFold(
        f.completeSuccess, e => recoverFn(e).voidFold(
          () => f.completeError(e), 
          recoverFuture => recoverFuture.onComplete(f.complete)
        ))
      );
      return f;
    }

    /* Waits until both futures yield a result. */
    public static Future<Tpl<A, B>> zip<A, B>(
      this Future<A> fa, Future<B> fb, string name=null
    ) {
      var fab = new FutureImpl<Tpl<A, B>>(name ?? $"({fa.name},{fb.name})");
      Act tryComplete = 
        () => fa.pureValue.zip(fb.pureValue).each(ab => fab.tryCompleteSuccess(ab));
      fa.onComplete(ta => ta.voidFold(_ => tryComplete(), e => fab.tryCompleteError(e)));
      fb.onComplete(tb => tb.voidFold(_ => tryComplete(), e => fab.tryCompleteError(e)));
      return fab;
    }

    public static IRxVal<Option<A>> toRxVal<A>(this Future<A> future) {
      var rx = RxRef.a(F.none<A>());
      future.onSuccess(a => rx.value = F.some(a));
      return rx;
    }
  }

  /** Coroutine based future **/
  public interface Future<A> {
    string name { get; }
    Option<Try<A>> value { get; }
    Option<A> pureValue { get; }
    Option<Exception> pureError { get; }
    /* If you are using onComplete, you must handle errors as well. */
    CancellationToken onComplete(Act<Try<A>> action);
    CancellationToken onSuccess(Act<A> action);
    CancellationToken onFailure(Act<Exception> action);
  }

  /**
   * You can use this token to cancel a callback before future is completed.
   **/
  public interface CancellationToken {
    bool isCancelled { get; }
    // Returns true if cancelled or false if already cancelled before.
    bool cancel();
  }

  /** Couroutine based promise **/
  public interface Promise<A> {
    /** Complete with value, exception if already completed. **/
    void complete(Try<A> v);
    void completeSuccess(A v);
    void completeError(Exception ex);
    /** Complete with value, return false if already completed. **/
    bool tryComplete(Try<A> v);
    bool tryCompleteSuccess(A v);
    bool tryCompleteError(Exception ex);
  }

  public static class Future {
    public class TimeoutException<A> : Exception {
      public readonly Future<A> future;
      public readonly float timeoutSeconds;

      public TimeoutException(Future<A> future, float timeoutSeconds)
      : base($"Future {future} timed out after {timeoutSeconds} seconds") {
        this.future = future;
        this.timeoutSeconds = timeoutSeconds;
      }
    }

    public static Future<A> a<A>(Act<Promise<A>> body, string name="[Future.a]") {
      var f = new FutureImpl<A>(name);
      body(f);
      return f;
    }

    public static Future<A> successful<A>(A value, string name="[Future.successful]") {
      var f = new FutureImpl<A>(name);
      f.completeSuccess(value);
      return f;
    }

    public static Future<A> failed<A>(Exception ex, string name="[Future.failed]") {
      var f = new FutureImpl<A>(name);
      f.completeError(ex);
      return f;
    }

    public static Future<A> unfullfiled<A>() { return UnfullfilledFutureImpl<A>.instance; }

    public static Future<A> delay<A>(float seconds, Fn<A> createValue, string name=null) {
      var f = new FutureImpl<A>(name ?? $"[Future.delay({seconds})]");
      ASync.WithDelay(seconds, () => f.complete(F.doTry(createValue)));
      return f;
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
      var future = new FutureImpl<A[]>(name ?? $"[Future.sequence({sourceFutures.Count})]");
      for (var idx = 0; idx < sourceFutures.Count; idx++) {
        var f = sourceFutures[idx]; 
        var fixedIdx = idx;
        f.onComplete(t => t.voidFold(
          value => {
            results[fixedIdx] = value;
            completed++;
            if (completed == results.Length) future.tryCompleteSuccess(results);
          },
          e => future.tryCompleteError(e)
        ));
      }
      return future;
    }

    /**
     * Returns result from the first future that completes.
     **/
    public static Future<A> firstOf<A>
    (this IEnumerable<Future<A>> enumerable, string name="[Future.firstOf]") {
      var future = new FutureImpl<A>(name);
      foreach (var f in enumerable) f.onComplete(v => future.tryComplete(v));
      return future;
    }

    /**
     * Returns result from the first future that completes. If all futures fail, 
     * returns the last error.
     **/
    public static Future<A> firstOfSuccessful<A>
    (this IEnumerable<Future<A>> enumerable, string name=null) {
      var futures = enumerable.ToList();
      var future = new FutureImpl<A>(name ?? $"[Future.firstOfSuccessful({futures.Count})]");
      var completions = 0;
      foreach (var f in futures) {
        f.onComplete(t => {
          completions++;
          t.voidFold(
            v => future.tryCompleteSuccess(v),
            ex => {
              if (completions == futures.Count) future.tryCompleteError(ex);
            }
          );
        });
      }
      return future;
    }

    public static Future<Unit> fromCoroutine(IEnumerator enumerator, string name="[Future.fromCoroutine]") {
      var f = new FutureImpl<Unit>(name);
      ASync.StartCoroutine(coroutineEnum(f, enumerator));
      return f;
    }

    /* Waits at most `timeoutSeconds` for the future to complete. Completes with 
       exception produced by `onTimeout` on timeout. */
    public static Future<A> timeout<A>(
      this Future<A> future, float timeoutSeconds, Fn<Exception> onTimeout
    ) {
      // TODO: test me
      var timeoutF = delay(timeoutSeconds, () => future.value.fold(
        // onTimeout() might have side effects, so we only need to execute it if 
        // there is no value in the original future once the timeout hits.
        () => F.throws<A>(onTimeout()),
        @try => @try.getOrThrow
      ));
      return new[] { future, timeoutF }.firstOf();
    }

    /* Waits at most `timeoutSeconds` for the future to complete. Completes with 
       TimeoutException<A> on timeout. */
    public static Future<A> timeout<A>(
      this Future<A> future, float timeoutSeconds
    ) {
      return future.timeout(timeoutSeconds, () => new TimeoutException<A>(future, timeoutSeconds));
    }

    private static IEnumerator coroutineEnum
    (Promise<Unit> p, IEnumerator enumerator) {
      yield return ASync.StartCoroutine(enumerator);
      p.completeSuccess(Unit.instance);
    }

    public class FinishedCancellationToken : CancellationToken {
      private static FinishedCancellationToken _instance;

      public static FinishedCancellationToken instance { get {
        return _instance ?? (_instance = new FinishedCancellationToken());
      } }

      private FinishedCancellationToken() {}

      public bool isCancelled { get { return true; } }
      public bool cancel() { return false; }
    }
  }

  /* Future that will never be fullfilled. */
  class UnfullfilledFutureImpl<A> : Future<A> {
    public readonly static Future<A> instance = new UnfullfilledFutureImpl<A>();
    UnfullfilledFutureImpl() {}

    public string name => "unfullfilled-future";
    public Option<Try<A>> value => F.none<Try<A>>();
    public Option<A> pureValue => F.none<A>();
    public Option<Exception> pureError => F.none<Exception>();
    public CancellationToken onComplete(Act<Try<A>> action) { return Future.FinishedCancellationToken.instance; }
    public CancellationToken onSuccess(Act<A> action) { return Future.FinishedCancellationToken.instance; }
    public CancellationToken onFailure(Act<Exception> action) { return Future.FinishedCancellationToken.instance; }
  }

  class FutureImpl<A> : Future<A>, Promise<A> {
    public class CancellationTokenImpl : CancellationToken {
      private readonly FutureListener listener;
      private readonly FutureImpl<A> future;
      public bool isCancelled { get; private set; }

      public CancellationTokenImpl(FutureListener listener, FutureImpl<A> future) {
        this.listener = listener;
        this.future = future;
        isCancelled = false;
      }

      public bool cancel() {
        isCancelled = true;
        return future.cancel(listener);
      }
    }

    public struct FutureListener {
      public readonly Act<Try<A>> handler;
      public readonly bool handlesErrors;

      public FutureListener(bool handlesErrors, Act<Try<A>> handler) {
        this.handlesErrors = handlesErrors;
        this.handler = handler;
      }
    }

    readonly IList<FutureListener> listeners = new List<FutureListener>();

    Option<Try<A>> _value;
    public string name { get; }
    public Option<Try<A>> value => _value;
    public Option<A> pureValue => _value.flatMap(t => t.toOption);
    public Option<Exception> pureError => _value.flatMap(t => t.exception);

    public FutureImpl(string name) {
      this.name = name;
      _value = F.none<Try<A>>();
    }

    public void complete(Try<A> v) {
      if (! tryComplete(v)) throw new IllegalStateException(string.Format(
        "Try to complete future with \"{0}\" but it is already " +
        "completed with \"{1}\"",
        v, value.get
      ));
    }

    public void completeSuccess(A v) { complete(F.scs(v)); }

    public void completeError(Exception ex) { complete(F.err<A>(ex)); }

    public bool tryComplete(Try<A> v) {
      // Cannot use fold here because of iOS AOT.
      var ret = value.isEmpty;
      if (ret) {
        // If no listeners are handling our errors - report them to log.
        if (!listeners.Any(l => l.handlesErrors))
          v.exception.each(e => Log.error($"Unhandled exception for future '{name}': {e.Message}", e));
        _value = F.some(v);
        // completed should be called only once
        completed(v);
      }
      return ret;
    }

    public bool tryCompleteSuccess(A v) {
      return tryComplete(F.scs(v));
    }

    public bool tryCompleteError(Exception ex) {
      return tryComplete(F.err<A>(ex));
    }

    private CancellationToken onComplete(FutureListener listener) {
      return value.fold<Try<A>, CancellationToken>(() => {
        listeners.Add(listener);
        return new CancellationTokenImpl(listener, this);
      }, v => {
        listener.handler(v);
        return Future.FinishedCancellationToken.instance;
      });
    }

    public CancellationToken onComplete(Act<Try<A>> action) {
      return onComplete(new FutureListener(true, action));
    }

    public CancellationToken onSuccess(Act<A> action) {
      return onComplete(new FutureListener(false, t => t.value.each(action)));
    }

    public CancellationToken onFailure(Act<Exception> action) {
      return onComplete(t => t.exception.each(action));
    }

    public void completed(Try<A> v) {
      foreach (var listener in listeners) listener.handler(v);
      listeners.Clear();
    }

    private bool cancel(FutureListener action) {
      return listeners.Remove(action);
    }
  }
}
