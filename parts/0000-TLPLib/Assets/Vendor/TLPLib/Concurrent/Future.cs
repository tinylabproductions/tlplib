using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class FutureExts {
    public static Future<B> map<A, B>(this Future<A> future, Fn<A, B> mapper) {
      var p = new FutureImpl<B>();
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
      this Future<A> future, Fn<A, Future<B>> mapper
    ) {
      var p = new FutureImpl<B>();
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
      this Future<A> future, Fn<Exception, Option<A>> recoverFn
    ) {
      var f = new FutureImpl<A>();
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
      this Future<A> future, Fn<Exception, Option<Future<A>>> recoverFn
    ) {
      var f = new FutureImpl<A>();
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
      this Future<A> fa, Future<B> fb
    ) {
      var fab = new FutureImpl<Tpl<A, B>>();
      Act tryComplete = 
        () => fa.pureValue.zip(fb.pureValue).each(ab => fab.tryCompleteSuccess(ab));
      fa.onComplete(ta => ta.voidFold(_ => tryComplete(), fab.completeError));
      fb.onComplete(tb => tb.voidFold(_ => tryComplete(), fab.completeError));
      return fab;
    }
  }

  /** Coroutine based future **/
  public interface Future<A> {
    Option<Try<A>> value { get; }
    Option<A> pureValue { get; }
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
    public static bool LOG_EXCEPTIONS = true;

    public static Future<A> successful<A>(A value) {
      var f = new FutureImpl<A>();
      f.completeSuccess(value);
      return f;
    }

    public static Future<A> failed<A>(Exception ex) {
      var f = new FutureImpl<A>();
      f.completeError(ex);
      return f;
    }

    public static Future<A> unfullfiled<A>() {
      return new FutureImpl<A>();
    }

    /**
     * Converts enumerable of futures into future of enumerable that is completed
     * when all futures complete.
     **/
    public static Future<A[]> sequence<A>(
      this IEnumerable<Future<A>> enumerable
    ) {
      var completed = 0u;
      var sourceFutures = enumerable.ToArray();
      var results = new A[sourceFutures.Length];
      var future = new FutureImpl<A[]>();
      for (var idx = 0; idx < sourceFutures.Length; idx++) {
        var f = sourceFutures[idx]; 
        var fixedIdx = idx;
        f.onSuccess(value => {
          results[fixedIdx] = value;
          completed++;
          if (completed == results.Length) future.tryCompleteSuccess(results);
        });
        f.onFailure(future.completeError);
      };
      return future;
    }

    /**
     * Returns result from the first future that completes.
     **/
    public static Future<A> firstOf<A>
    (this IEnumerable<Future<A>> enumerable) {
      var future = new FutureImpl<A>();
      foreach (var f in enumerable) f.onComplete(v => future.tryComplete(v));
      return future;
    }

    /**
     * Returns result from the first future that completes. If all futures fail, 
     * returns the last error.
     **/
    public static Future<A> firstOfSuccessful<A>
    (this IEnumerable<Future<A>> enumerable) {
      var future = new FutureImpl<A>();
      var completions = 0;
      var futures = enumerable.ToList();
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

    public static Future<Unit> fromCoroutine(IEnumerator enumerator) {
      var f = new FutureImpl<Unit>();
      ASync.StartCoroutine(coroutineEnum(f, enumerator));
      return f;
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

  class FutureImpl<A> : Future<A>, Promise<A> {
    public class CancellationTokenImpl : CancellationToken {
      private readonly Act<Try<A>> action;
      private readonly FutureImpl<A> future;
      public bool isCancelled { get; private set; } 

      public CancellationTokenImpl(Act<Try<A>> action, FutureImpl<A> future) {
        this.action = action;
        this.future = future;
        isCancelled = false;
      }

      public bool cancel() {
        isCancelled = true;
        return future.cancel(action);
      }
    }

    private readonly IList<Act<Try<A>>> listeners = new List<Act<Try<A>>>();

    private Option<Try<A>> _value = F.none<Try<A>>();
    public Option<Try<A>> value { get { return _value; } }
    public Option<A> pureValue 
    { get { return _value.flatMap(t => t.fold(F.some, _ => F.none<A>())); } }

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
        if (Future.LOG_EXCEPTIONS) v.exception.each(Log.error);
        _value = F.some(v);
      }
      completed(v);
      return ret;
    }

    public bool tryCompleteSuccess(A v) {
      return tryComplete(F.scs(v));
    }

    public bool tryCompleteError(Exception ex) {
      return tryComplete(F.err<A>(ex));
    }

    public CancellationToken onComplete(Act<Try<A>> action) {
      return value.fold<Try<A>, CancellationToken>(() => {
        listeners.Add(action);
        return new CancellationTokenImpl(action, this);
      }, v => {
        action(v);
        return Future.FinishedCancellationToken.instance;
      });
    }

    public CancellationToken onSuccess(Act<A> action) {
      return onComplete(t => t.value.each(action));
    }

    public CancellationToken onFailure(Act<Exception> action) {
      return onComplete(t => t.exception.each(action));
    }

    public void completed(Try<A> v) {
      foreach (var listener in listeners) listener(v);
      listeners.Clear();
    }

    private bool cancel(Act<Try<A>> action) {
      return listeners.Remove(action);
    }
  }
}
