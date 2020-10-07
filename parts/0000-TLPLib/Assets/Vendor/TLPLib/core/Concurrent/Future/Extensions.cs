using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using pzd.lib.reactive;

using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.dispose;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [PublicAPI] public static class FutureExts {
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

    public static Future<Unit> discardValue<A>(this Future<A> future) => future.map(_ => Unit._);

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
  }
}
