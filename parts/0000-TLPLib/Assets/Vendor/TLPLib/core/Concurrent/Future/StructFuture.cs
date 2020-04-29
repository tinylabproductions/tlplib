using System;
using pzd.lib.concurrent;
using pzd.lib.functional;
using pzd.lib.functional.higher_kinds;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public enum FutureType : byte { Successful, Unfulfilled, ASync }

  /// <summary>
  /// Struct based future which does not generate garbage if it's actually synchronous.
  /// </summary>
  public readonly struct Future<A> : IEquatable<Future<A>>, HigherKind<Future.W, A> {
    public readonly FutureType type;
    public readonly A __unsafeGetSuccessful;
    public readonly IHeapFuture<A> __unsafeGetHeapFuture;

    Future(FutureType type, A successfulFuture, IHeapFuture<A> heapFuture) {
      this.type = type;
      __unsafeGetSuccessful = successfulFuture;
      __unsafeGetHeapFuture = heapFuture;
    }

    public Future(A value) : this(FutureType.Successful, value, null) {}
    public static Future<A> unfulfilled => new Future<A>(FutureType.Unfulfilled, default, null);
    public Future(IHeapFuture<A> future) : this(FutureType.ASync, default, future) {}

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

    public override string ToString() {
      var header = $"Future<{typeof (A)}>";
      return type switch {
        FutureType.Successful => $"{header}.Successful({__unsafeGetSuccessful})",
        FutureType.Unfulfilled => $"{header}.Unfulfilled",
        FutureType.ASync => $"{header}.ASync({__unsafeGetHeapFuture.value})",
        _ => throw new Exception("developer error")
      };
    }

    public bool isCompleted => type switch {
      FutureType.Successful => true,
      FutureType.Unfulfilled => false,
      FutureType.ASync => __unsafeGetHeapFuture.isCompleted,
      _ => throw new Exception("developer error")
    };
      
    public Option<A> value => type switch {
      FutureType.Successful => Some.a(__unsafeGetSuccessful),
      FutureType.Unfulfilled => None._,
      FutureType.ASync => __unsafeGetHeapFuture.value,
      _ => throw new Exception("developer error")
    };
  }
}
