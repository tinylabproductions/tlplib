using System;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [PublicAPI] public static class FutureExts {
    public static Future<A> flatten<A>(this Future<Future<A>> future) =>
      future.flatMap(_ => _);

    /// <summary>
    /// Complete the future with the right side, never complete if left side occurs.
    /// </summary>
    public static Future<B> dropError<A, B>(
      this Future<Functional.Either<A, B>> future, bool logOnError = false
    ) =>
      Future.a<B>(p => future.onComplete(either => either.voidFold(
        err => { if (logOnError) Log.d.error(err.ToString()); },
        p.complete
      )));

    public static ISubscription onCompleteCancellable<A>(this Future<A> future, Action<A> action) {
      var sub = new Subscription(() => { });
      future.onComplete(val => { if (sub.isSubscribed) action(val); });
      return sub;
    }

    public static IRxVal<Functional.Option<A>> toRxVal<A>(this Future<A> future) {
      var rx = RxRef.a(F.none<A>());
      future.onComplete(a => rx.value = F.some(a));
      return rx;
    }

    public static IRxVal<A> toRxVal<A>(this Future<A> future, A whileNotCompleted) {
      var rx = RxRef.a(whileNotCompleted);
      future.onComplete(a => rx.value = a);
      return rx;
    }

    public static IRxVal<B> toRxVal<A, B>(this Future<A> future, B whileNotCompleted, Func<A, B> onCompletion) {
      var rx = RxRef.a(whileNotCompleted);
      future.onComplete(a => rx.value = onCompletion(a));
      return rx;
    }

    public static IRxVal<A> toRxVal<A>(
      this Future<IRxVal<A>> future, A whileNotCompleted
    ) => new RxVal<A>(
      whileNotCompleted,
      setValue => {
        var sub = Subscription.empty;
        future.onComplete(rx2 => {
          sub = rx2.subscribe(NoOpDisposableTracker.instance, a => setValue(a));
        });
        return new Subscription(() => sub.unsubscribe());
      }
    );

    public static Future<Functional.Either<A, B>> extract<A, B>(this Functional.Either<A, Future<B>> eitherFuture) =>
      eitherFuture.fold(
        a => Future.successful(Functional.Either<A, B>.Left(a)),
        bFuture => bFuture.map(Functional.Either<A, B>.Right)
      );

    public static Future<Try<A>> extract<A>(this Try<Future<Try<A>>> tryFuture) =>
      tryFuture.fold(
        future => future,
        exception => Future.successful<Try<A>>(exception)
      );

    public static Future<A> extract<A>(this Functional.Option<Future<A>> futureOpt) =>
      futureOpt.fold(Future<A>.unfulfilled, f => f);

    public static Future<A> extract<A>(this Future<Functional.Option<A>> optFuture) =>
      optFuture.flatMap(opt => opt.fold(Future<A>.unfulfilled, Future.successful));

    public static Future<Functional.Option<A>> extractOpt<A>(this Functional.Option<Future<A>> futureOpt) =>
      futureOpt.fold(() => Future.successful(F.none<A>()), f => f.map(F.some));

    [PublicAPI]
    public static void onComplete<A, B>(
      this Future<Functional.Either<A, B>> future,
      Action<A> onError, Action<B> onSuccess
    ) =>
      future.onComplete(e => {
        if (e.isLeft) onError(e.__unsafeGetLeft);
        else onSuccess(e.__unsafeGetRight);
      });

    [PublicAPI]
    public static void onSuccess<A, B>(this Future<Functional.Either<A, B>> future, Action<B> action) =>
      future.onComplete(e => {
        foreach (var b in e.rightValue) action(b);
      });

    [PublicAPI]
    public static void onSuccess<A>(this Future<Try<A>> future, Action<A> action) =>
      future.onComplete(e => {
        foreach (var a in e.value) action(a);
      });

    [PublicAPI]
    public static Future<Functional.Option<B>> ofSuccess<A, B>(this Future<Functional.Either<A, B>> future) =>
      future.map(e => e.rightValue);

    [PublicAPI]
    public static Future<Functional.Option<A>> ofSuccess<A>(this Future<Try<A>> future) =>
      future.map(e => e.value);

    public static void onFailure<A, B>(this Future<Functional.Either<A, B>> future, Action<A> action) =>
      future.onComplete(e => {
        foreach (var a in e.leftValue) action(a);
      });

    public static void onFailure<A>(this Future<Try<A>> future, Action<Exception> action) =>
      future.onComplete(e => {
        foreach (var ex in e.exception) action(ex);
      });

    public static Future<Functional.Option<A>> ofFailure<A, B>(this Future<Functional.Either<A, B>> future) =>
      future.map(e => e.leftValue);

    public static Future<Functional.Option<Exception>> ofFailure<A>(this Future<Try<A>> future) =>
      future.map(e => e.exception);

    /**
     * Delays completing of given future until the returned action is called.
     **/
    public static Tpl<Future<A>, Action> delayUntilSignal<A>(this Future<A> future) {
      Promise<Unit> signalP;
      var f = future.zip(Future<Unit>.async(out signalP), (a, _) => a);
      Action act = () => signalP.tryComplete(F.unit);
      return F.t(f, act);
    }

    /** Converts option into successful/unfulfilled future. */
    public static Future<A> toFuture<A>(this Functional.Option<A> opt) {
      foreach (var a in opt) return Future.successful(a);
      return Future<A>.unfulfilled;
    }

    public static LazyVal<Functional.Option<A>> toLazy<A>(this Future<A> f) =>
      F.lazy(() => f.value);

    public static Future<A> first<A>(this Future<A> a, Future<A> b) {
      return Future.a<A>(p => {
        a.onComplete(_ => p.tryComplete(_));
        b.onComplete(_ => p.tryComplete(_));
      });
    }
  }
}
