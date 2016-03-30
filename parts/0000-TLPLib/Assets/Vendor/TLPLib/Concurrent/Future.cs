using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class FutureExts {
    public static Future<B> map<A, B>(this Future<A> future, Fn<A, B> mapper) {
      return Future.a<B>(p => future.onComplete(v => p.complete(mapper(v))));
    }

    public static Future<Either<Err, To>> mapE<From, To, Err>(
      this Future<Either<Err, From>> future, Fn<From, To> mapper
    ) { return future.map(e => e.mapRight(mapper)); }

    public static Future<Either<Err, To>> mapE<From, To, Err>(
      this Future<Either<Err, From>> future, Fn<From, Either<Err, To>> mapper
    ) { return future.map(e => e.flatMapRight(mapper)); }

    public static Future<Option<To>> mapO<From, To>(
      this Future<Option<From>> future, Fn<From, To> mapper
    ) { return future.map(opt => opt.map(mapper)); }

    public static Future<B> flatMap<A, B>(
      this Future<A> future, Fn<A, Future<B>> mapper
    ) {
      return Future.a<B>(p => 
        future.onComplete(v => mapper(v).onComplete(p.complete))
      );
    }

    public static Future<Option<B>> flatMapO<A, B>(
      this Future<Option<A>> future, Fn<A, Future<Option<B>>> mapper
    ) {
      return future.flatMap(opt => opt.fold(
        () => Future.successful(F.none<B>()),
        mapper
      ));
    }

    public static Future<Either<Err, To>> flatMapE<From, To, Err>(
      this Future<Either<Err, From>> future, Fn<From, Future<To>> mapper
    ) {
      return future.flatMap(e => e.fold(
        err => Future.successful(Either<Err, To>.Left(err)),
        from => mapper(from).map(Either<Err, To>.Right)
      ));
    }

    public static Future<Either<Err, To>> flatMapE<From, To, Err>(
      this Future<Either<Err, From>> future, Fn<From, Future<Either<Err, To>>> mapper
    ) {
      return future.flatMap(e => e.fold(
        err => Future.successful(Either<Err, To>.Left(err)),
        mapper
      ));
    }

    public static Future<Try<To>> flatMapT<From, To>(
      this Future<Try<From>> future, Fn<From, Future<To>> mapper
    ) {
      return future.flatMap(t => t.fold(
        from => mapper(from).map(F.scs),
        err => Future.successful(F.err<To>(err))
      ));
    }

    /** Complete the future with the right side, never complete if left side occurs. **/
    public static Future<B> dropError<A, B>(this Future<Either<A, B>> future) {
      return Future.a<B>(p => future.onSuccess(p.complete));
    }

    /* Waits until both futures yield a result. */
    public static Future<Tpl<A, B>> zip<A, B>(
      this Future<A> fa, Future<B> fb, string name=null
    ) {
      var fab = new FutureImpl<Tpl<A, B>>();
      Act tryComplete = () => fa.value.zip(fb.value).each(ab => fab.tryComplete(ab));
      fa.onComplete(a => tryComplete());
      fb.onComplete(b => tryComplete());
      return fab;
    }

    public static IRxVal<Option<A>> toRxVal<A>(this Future<A> future) {
      var rx = RxRef.a(F.none<A>());
      future.onComplete(a => rx.value = F.some(a));
      return rx;
    }

    public static CancellationToken onSuccess<A, B>(this Future<Either<A, B>> future, Act<B> action)
      { return future.onComplete(e => e.rightValue.each(action)); }

    public static CancellationToken onFailure<A, B>(this Future<Either<A, B>> future, Act<A> action)
      { return future.onComplete(e => e.leftValue.each(action)); }
  }

  /** Coroutine based future **/
  public interface Future<A> {
    Option<A> value { get; }
    CancellationToken onComplete(Act<A> action);
  }

  /**
   * You can use this token to cancel a callback before future is completed.
   **/
  public interface CancellationToken {
    bool isCancelled { get; }
    // Returns true if cancelled or false if already cancelled before.
    bool cancel();
  }

  public static class PromiseExts {
    public static void completeSuccess<Err, Val>(this Promise<Either<Err, Val>> p, Val value) {
      p.complete(Either<Err, Val>.Right(value));
    }

    public static void completeSuccess<Val>(this Promise<Try<Val>> p, Val value) {
      p.complete(F.scs(value));
    }

    public static void tryCompleteSuccess<Err, Val>(this Promise<Either<Err, Val>> p, Val value) {
      p.tryComplete(Either<Err, Val>.Right(value));
    }

    public static void tryCompleteSuccess<Val>(this Promise<Try<Val>> p, Val value) {
      p.tryComplete(F.scs(value));
    }

    public static void completeError<Err, Val>(this Promise<Either<Err, Val>> p, Err error) {
      p.complete(Either<Err, Val>.Left(error));
    }

    public static void completeError<Val>(this Promise<Try<Val>> p, Exception error) {
      p.complete(F.err<Val>(error));
    }

    public static void tryCompleteError<Err, Val>(this Promise<Either<Err, Val>> p, Err error) {
      p.tryComplete(Either<Err, Val>.Left(error));
    }

    public static void tryCompleteError<Val>(this Promise<Try<Val>> p, Exception error) {
      p.tryComplete(F.err<Val>(error));
    }
  }

  /** Couroutine based promise **/
  public interface Promise<in A> {
    /** Complete with value, exception if already completed. **/
    void complete(A v);
    /** Complete with value, return false if already completed. **/
    bool tryComplete(A v);
  }

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

      public TimeoutException(Timeout timeout) : base($"Future timed out: {timeout}") {}
    }

    public static Future<A> a<A>(Act<Promise<A>> body) {
      var f = new FutureImpl<A>();
      body(f);
      return f;
    }

    public static Future<A> successful<A>(A value) {
      return new SuccessfulFutureImpl<A>(value);
    }

    public static Future<A> unfullfiled<A>() { return UnfullfilledFutureImpl<A>.instance; }

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
      var future = new FutureImpl<A[]>();
      for (var idx = 0; idx < sourceFutures.Count; idx++) {
        var f = sourceFutures[idx]; 
        var fixedIdx = idx;
        f.onComplete(value => {
          results[fixedIdx] = value;
          completed++;
          if (completed == results.Length) future.tryComplete(results);
        });
      }
      return future;
    }

    /**
     * Returns result from the first future that completes.
     **/
    public static Future<A> firstOf<A>(this IEnumerable<Future<A>> enumerable) {
      var future = new FutureImpl<A>();
      foreach (var f in enumerable) f.onComplete(v => future.tryComplete(v));
      return future;
    }

    /**
     * Returns result from the first future that satisfies the predicate as a Some. 
     * If all futures do not satisfy the predicate returns None.
     **/
    public static Future<Option<B>> firstOfWhere<A, B>
    (this IEnumerable<Future<A>> enumerable, Fn<A, Option<B>> predicate) {
      var futures = enumerable.ToList();
      var future = new FutureImpl<Option<B>>();

      var completed = 0;
      foreach (var f in futures)
        f.onComplete(a => {
          completed++;
          var res = predicate(a);
          if (res.isDefined) future.tryComplete(res);
          else if (completed == futures.Count) future.tryComplete(F.none<B>());
        });
      return future;
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
      var f = new FutureImpl<Unit>();
      ASync.StartCoroutine(coroutineEnum(f, enumerator));
      return f;
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

    public class FinishedCancellationToken : CancellationToken {
      public static readonly FinishedCancellationToken instance = new FinishedCancellationToken();
      FinishedCancellationToken() {}

      public bool isCancelled { get { return true; } }
      public bool cancel() { return false; }
    }
  }

  class SuccessfulFutureImpl<A> : Future<A> {
    readonly A _value;

    public SuccessfulFutureImpl(A value) {
      _value = value;
    }

    public Option<A> value => _value.some();

    public CancellationToken onComplete(Act<A> action) {
      action(_value);
      return Future.FinishedCancellationToken.instance;
    }
  }

  /* Future that will never be fullfilled. */
  class UnfullfilledFutureImpl<A> : Future<A> {
    public static readonly Future<A> instance = new UnfullfilledFutureImpl<A>();
    UnfullfilledFutureImpl() {}

    public Option<A> value => F.none<A>();
    public CancellationToken onComplete(Act<A> action) { return Future.FinishedCancellationToken.instance; }
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
      public readonly Act<A> handler;

      public FutureListener(Act<A> handler) {
        this.handler = handler;
      }
    }

    readonly IList<FutureListener> listeners = new List<FutureListener>();

    public Option<A> value { get; private set; }

    public FutureImpl() { value = F.none<A>(); }

    public void complete(A v) {
      if (! tryComplete(v)) throw new IllegalStateException(
        $"Try to complete future with \"{v}\" but it is already " + $"completed with \"{value.get}\""
      );
    }

    public bool tryComplete(A v) {
      // Cannot use fold here because of iOS AOT.
      var ret = value.isEmpty;
      if (ret) {
        value = F.some(v);
        // completed should be called only once
        completed(v);
      }
      return ret;
    }

    CancellationToken onComplete(FutureListener listener) {
      return value.fold<A, CancellationToken>(() => {
        listeners.Add(listener);
        return new CancellationTokenImpl(listener, this);
      }, v => {
        listener.handler(v);
        return Future.FinishedCancellationToken.instance;
      });
    }

    public CancellationToken onComplete(Act<A> action) {
      return onComplete(new FutureListener(action));
    }

    public void completed(A v) {
      foreach (var listener in listeners) listener.handler(v);
      listeners.Clear();
    }

    bool cancel(FutureListener action) {
      return listeners.Remove(action);
    }
  }
}
