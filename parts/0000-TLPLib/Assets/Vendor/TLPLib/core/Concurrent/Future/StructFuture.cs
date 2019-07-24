using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.concurrent;
using pzd.lib.functional;
using pzd.lib.functional.higher_kinds;

namespace com.tinylabproductions.TLPLib.Concurrent {
  class UnfulfilledFuture : IEquatable<UnfulfilledFuture> {
    #region Equality
    public bool Equals(UnfulfilledFuture other) => true;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is UnfulfilledFuture future && Equals(future);
    }

    public override int GetHashCode() => nameof(UnfulfilledFuture).GetHashCode();

    public static bool operator ==(UnfulfilledFuture left, UnfulfilledFuture right) { return left.Equals(right); }
    public static bool operator !=(UnfulfilledFuture left, UnfulfilledFuture right) { return !left.Equals(right); }
    #endregion
  }
  public enum FutureType { Successful, Unfulfilled, ASync }

  /**
   * class based future which does not generate garbage if it's actually
   * synchronous.
   **/
  public class Future<A> : IEquatable<Future<A>>, HigherKind<Future.W, A> {
    /* Future with a known value|unfulfilled future|async future. */
    readonly OneOf<A, UnfulfilledFuture, IHeapFuture<A>> implementation;
    public bool isCompleted => implementation.fold(_ => true, _ => false, f => f.isCompleted);
    // ReSharper disable once ConvertClosureToMethodGroup
    public Option<A> value => implementation.fold(_ => Some.a(_), _ => None._, f => f.value);

    public FutureType type => implementation.fold(
      _ => FutureType.Successful,
      _ => FutureType.Unfulfilled,
      _ => FutureType.ASync
    );

    Future(OneOf<A, UnfulfilledFuture, IHeapFuture<A>> implementation) {
      this.implementation = implementation;
    }

    public Future(IHeapFuture<A> future) : this(new OneOf<A, UnfulfilledFuture, IHeapFuture<A>>(future)) {}

    #region Equality

    public bool Equals(Future<A> other) => value.Equals(other.value);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Future<A> future && Equals(future);
    }

    public override int GetHashCode() => value.GetHashCode();

    public static bool operator ==(Future<A> left, Future<A> right) { return left.Equals(right); }
    public static bool operator !=(Future<A> left, Future<A> right) { return !left.Equals(right); }

    #endregion

    /// <summary>Lift an ordinary value into a future.</summary>
    public static Future<A> successful(A value) => new Future<A>(value);

    /// <summary>Future that will never be completed.</summary>
    public static readonly Future<A> unfulfilled = new Future<A>(new UnfulfilledFuture());

    /// <summary>Asynchronous heap based future which can be completed later.</summary>
    public static Future<A> async(Action<Promise<A>> body) => async((p, _) => body(p));

    /// <summary>Asynchronous heap based future which can be completed later.</summary>
    public static Future<A> async(Action<Promise<A>, Future<A>> body) {
      var future = async(out var promise);
      body(promise, future);
      return future;
    }

    public static Future<A> async(out Promise<A> promise) {
      var impl = new FutureImpl<A>();
      promise = impl;
      return new Future<A>(new OneOf<A, UnfulfilledFuture, IHeapFuture<A>>(impl));
    }

    public override string ToString() {
      var header = $"Future<{typeof (A)}>";
      if (implementation.isA) return $"{header}.Successful({implementation.__unsafeGetA})";
      if (implementation.isB) return $"{header}.Unfulfilled";
      if (implementation.isC) return $"{header}.ASync({implementation.__unsafeGetC.value})";
      throw new IllegalStateException();
    }

    public void onComplete(Action<A> action) {
      if (implementation.isA) action(implementation.__unsafeGetA);
      else if (implementation.isC) implementation.__unsafeGetC.onComplete(action);
    }

    /**
     * Always run `action`. If the future is not completed right now, run `action` again when it
     * completes.
     */
    public void nowAndOnComplete(Action<Option<A>> action) {
      var current = value;
      action(current);
      if (current.isNone) onComplete(a => action(a.some()));
    }

    public Future<B> map<B>(Func<A, B> mapper) => implementation.fold(
      v => Future<B>.successful(mapper(v)),
      _ => Future<B>.unfulfilled,
      f => Future<B>.async(p => f.onComplete(v => p.complete(mapper(v))))
    );

    public Future<B> flatMap<B>(Func<A, Future<B>> mapper) => implementation.fold(
      mapper,
      _ => Future<B>.unfulfilled,
      f => Future<B>.async(p => f.onComplete(v => mapper(v).onComplete(p.complete)))
    );

    public Future<C> flatMap<B, C>(Func<A, Future<B>> mapper, Func<A, B, C> joiner) => implementation.fold(
      a => mapper(a).map(b => joiner(a, b)),
      _ => Future<C>.unfulfilled,
      f => Future<C>.async(p => f.onComplete(a => mapper(a).onComplete(b => p.complete(joiner(a, b)))))
    );

    /** Filter future on value - if predicate matches turns completed future into unfulfilled. **/
    public Future<A> filter(Func<A, bool> predicate) {
      var self = this;
      return implementation.fold(
        a => predicate(a) ? self : unfulfilled,
        _ => self,
        f => async(p => f.onComplete(a => { if (predicate(a)) p.complete(a); }))
      );
    }

    /**
     * Filter & map future on value. If collector returns Some, completes the future,
     * otherwise - never completes.
     **/
    public Future<B> collect<B>(Func<A, Option<B>> collector) {
      return implementation.fold(
        a => collector(a).fold(Future<B>.unfulfilled, Future<B>.successful),
        _ => Future<B>.unfulfilled,
        f => Future<B>.async(p => f.onComplete(a => {
          foreach (var b in collector(a)) p.complete(b);
        }))
      );
    }

    /* Waits until both futures yield a result. */
    public Future<Tpl<A, B>> zip<B>(Future<B> fb) => zip(fb, F.t);

    public Future<C> zip<B, C>(Future<B> fb, Func<A, B, C> mapper) {
      if (implementation.isB || fb.implementation.isB) return Future<C>.unfulfilled;
      if (implementation.isA && fb.implementation.isA)
        return Future.successful(mapper(implementation.__unsafeGetA, fb.implementation.__unsafeGetA));

      var fa = this;
      return Future<C>.async(p => {
        void tryComplete() {
          if (fa.value.valueOut(out var a) && fb.value.valueOut(out var b))
            p.tryComplete(mapper(a, b));
        }

        fa.onComplete(a => tryComplete());
        fb.onComplete(b => tryComplete());
      });
    }
  }
}
