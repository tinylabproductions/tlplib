﻿using System;
using pzd.lib.concurrent;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class Cancellable {
    public static Cancellable<A> a<A>(A value, Func<bool> cancel) =>
      new Cancellable<A>(value, cancel);

    public static Future<A> asNonCancellable<A>(
      this Cancellable<Future<Either<Cancelled, A>>> cancellable
    ) => cancellable.value.flatMap(e => e.fold(
      cancelled => Future<A>.unfulfilled,
      Future.successful
    ));
  }

  public struct Cancellable<A> {
    public readonly A value;
    readonly Func<bool> _cancel;

    public Cancellable(A value, Func<bool> cancel) {
      this.value = value;
      _cancel = cancel;
    }

    public Cancellable<B> map<B>(Func<A, B> mapper) =>
      Cancellable.a(mapper(value), cancel);

    /** true if cancelled, false is cancelling is impossible (for example for completed WWW). */
    public bool cancel() => _cancel();
  }

  /** Marker that an operation was cancelled. */
  public struct Cancelled : IEquatable<Cancelled> {
    public static readonly Cancelled instance = new Cancelled();

    public override string ToString() => nameof(Cancelled);

    #region Equality

    public bool Equals(Cancelled other) => true;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Cancelled && Equals((Cancelled) obj);
    }

    public override int GetHashCode() => 584154;
    public static bool operator ==(Cancelled left, Cancelled right) => left.Equals(right);
    public static bool operator !=(Cancelled left, Cancelled right) => !left.Equals(right);

    #endregion
  }
}
