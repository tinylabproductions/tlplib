using System;

using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.data;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.reactive;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [PublicAPI] public static class FutureExts {
    public static void onComplete<A>(this Future<A> f, Action<A> action) {
      if (f.type == FutureType.Successful) action(f.__unsafeGetSuccessful);
      else if (f.type == FutureType.ASync) f.__unsafeGetHeapFuture.onComplete(action);
    }

    public static Future<B> map<A, B>(this Future<A> f, Func<A, B> mapper) => f.type switch {
      FutureType.Successful => Future.successful(mapper(f.__unsafeGetSuccessful)),
      FutureType.Unfulfilled => Future<B>.unfulfilled,
      FutureType.ASync => Future.async<B>(p => f.__unsafeGetHeapFuture.onComplete(v => p.complete(mapper(v)))),
      _ => throw new DeveloperError("unreachable") 
    };

    public static Future<B> flatMap<A, B>(this Future<A> f, Func<A, Future<B>> mapper) => f.type switch {
      FutureType.Successful => mapper(f.__unsafeGetSuccessful),
      FutureType.Unfulfilled => Future<B>.unfulfilled,
      FutureType.ASync => Future.async<B>(p =>
        f.__unsafeGetHeapFuture.onComplete(v => mapper(v).onComplete(p.complete))
      ),
      _ => throw new DeveloperError("unreachable")
    };

    public static Future<C> flatMap<A, B, C>(this Future<A> f, Func<A, Future<B>> mapper, Func<A, B, C> joiner) =>
      f.type switch {
        FutureType.Successful => mapper(f.__unsafeGetSuccessful).map(b => joiner(f.__unsafeGetSuccessful, b)),
        FutureType.Unfulfilled => Future<C>.unfulfilled,
        FutureType.ASync => Future.async<C>(p => 
          f.__unsafeGetHeapFuture.onComplete(a => mapper(a).onComplete(b => p.complete(joiner(a, b))))
        ),
        _ => throw new DeveloperError("unreachable")
      };

    /// <summary>
    /// Filter future on value - if predicate matches turns completed future into unfulfilled.
    /// </summary>
    public static Future<A> filter<A>(this Future<A> f, Func<A, bool> predicate) =>
      f.type switch {
        FutureType.Successful => predicate(f.__unsafeGetSuccessful) ? f : Future<A>.unfulfilled,
        FutureType.Unfulfilled => f,
        FutureType.ASync => Future.async<A>(p => f.onComplete(a => { if (predicate(a)) p.complete(a); })),
        _ => throw new DeveloperError("unreachable")
      };

    /// <summary>
    /// Filter & map future on value. If collector returns Some, completes the future, otherwise - never completes.
    /// </summary>
    public static Future<B> collect<A, B>(this Future<A> f, Func<A, Option<B>> collector) =>
      f.type switch {
        FutureType.Successful => collector(f.__unsafeGetSuccessful).fold(Future<B>.unfulfilled, Future.successful),
        FutureType.Unfulfilled => Future<B>.unfulfilled,
        FutureType.ASync => Future.async<B>(p => f.onComplete(a => {
          foreach (var b in collector(a)) p.complete(b);
        })),
        _ => throw new DeveloperError("unreachable")
      };

    /// <summary>
    /// Waits until both futures yield a result.
    /// </summary>
    public static Future<Tpl<A, B>> zip<A, B>(this Future<A> f, Future<B> fb) => f.zip(fb, F.t);

    public static Future<C> zip<A, B, C>(this Future<A> fa, Future<B> fb, Func<A, B, C> mapper) {
      if (fa.type == FutureType.Unfulfilled || fb.type == FutureType.Unfulfilled) return Future<C>.unfulfilled;
      if (fa.type == FutureType.Successful && fb.type == FutureType.Successful)
        return Future.successful(mapper(fa.__unsafeGetSuccessful, fb.__unsafeGetSuccessful));

      return Future.async<C>(p => {
        void tryComplete() {
          if (fa.value.valueOut(out var a) && fb.value.valueOut(out var b))
            p.tryComplete(mapper(a, b));
        }

        fa.onComplete(a => tryComplete());
        fb.onComplete(b => tryComplete());
      });
    }
    
    /// <summary>
    /// Always run `action`. If the future is not completed right now, run `action` again when it completes.
    /// </summary>
    public static void nowAndOnComplete<A>(this Future<A> future, Action<Option<A>> action) {
      var current = future.value;
      action(current);
      if (current.isNone) future.onComplete(a => action(Some.a(a)));
    }
    
    public static Future<A> flatten<A>(this Future<Future<A>> future) =>
      future.flatMap(_ => _);

    /// <summary>
    /// Complete the future with the right side, never complete if left side occurs.
    /// </summary>
    public static Future<B> dropError<A, B>(
      this Future<Either<A, B>> future, bool logOnError = false
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

    public static IRxVal<Option<A>> toRxVal<A>(this Future<A> future) {
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

    public static Future<Either<A, B>> extract<A, B>(this Either<A, Future<B>> eitherFuture) =>
      eitherFuture.fold(
        a => Future.successful(Either<A, B>.Left(a)),
        bFuture => bFuture.map(Either<A, B>.Right)
      );

    public static Future<Try<A>> extract<A>(this Try<Future<Try<A>>> tryFuture) =>
      tryFuture.fold(
        future => future,
        exception => Future.successful<Try<A>>(exception)
      );

    public static Future<A> extract<A>(this Option<Future<A>> futureOpt) =>
      futureOpt.fold(Future<A>.unfulfilled, f => f);

    public static Future<A> extract<A>(this Future<Option<A>> optFuture) =>
      optFuture.flatMap(opt => opt.fold(Future<A>.unfulfilled, Future.successful));

    public static Future<Option<A>> extractOpt<A>(this Option<Future<A>> futureOpt) =>
      futureOpt.fold(() => Future.successful(F.none<A>()), f => f.map(F.some));

    [PublicAPI]
    public static void onComplete<A, B>(
      this Future<Either<A, B>> future,
      Action<A> onError, Action<B> onSuccess
    ) =>
      future.onComplete(e => {
        if (e.isLeft) onError(e.__unsafeGetLeft);
        else onSuccess(e.__unsafeGetRight);
      });

    [PublicAPI]
    public static void onSuccess<A, B>(this Future<Either<A, B>> future, Action<B> action) =>
      future.onComplete(e => {
        foreach (var b in e.rightValue) action(b);
      });

    [PublicAPI]
    public static void onSuccess<A>(this Future<Try<A>> future, Action<A> action) =>
      future.onComplete(e => {
        if (e.valueOut(out var a)) action(a);
      });

    [PublicAPI]
    public static Future<Option<B>> ofSuccess<A, B>(this Future<Either<A, B>> future) =>
      future.map(e => e.rightValue);

    [PublicAPI]
    public static Future<Option<A>> ofSuccess<A>(this Future<Try<A>> future) =>
      future.map(e => e.toOption());

    public static void onFailure<A, B>(this Future<Either<A, B>> future, Action<A> action) =>
      future.onComplete(e => {
        foreach (var a in e.leftValue) action(a);
      });

    public static void onFailure<A>(this Future<Try<A>> future, Action<Exception> action) =>
      future.onComplete(e => {
        foreach (var ex in e.exception()) action(ex);
      });

    public static Future<Option<A>> ofFailure<A, B>(this Future<Either<A, B>> future) =>
      future.map(e => e.leftValue);

    public static Future<Option<Exception>> ofFailure<A>(this Future<Try<A>> future) =>
      future.map(e => e.exception());

    /// <summary>
    /// Delays completing of given future until the returned action is called.
    /// </summary>
    public static Tpl<Future<A>, Action> delayUntilSignal<A>(this Future<A> future) {
      var f = future.zip(Future.async(out Promise<Unit> signalP), (a, _) => a);
      void act() => signalP.tryComplete(F.unit);
      return F.t(f, (Action) act);
    }

    /** Converts option into successful/unfulfilled future. */
    public static Future<A> toFuture<A>(this Option<A> opt) {
      foreach (var a in opt) return Future.successful(a);
      return Future<A>.unfulfilled;
    }

    public static LazyVal<Option<A>> toLazy<A>(this Future<A> f) =>
      F.lazy(() => f.value);

    public static Future<A> first<A>(this Future<A> a, Future<A> b) {
      return Future.a<A>(p => {
        a.onComplete(_ => p.tryComplete(_));
        b.onComplete(_ => p.tryComplete(_));
      });
    }
  }
}
