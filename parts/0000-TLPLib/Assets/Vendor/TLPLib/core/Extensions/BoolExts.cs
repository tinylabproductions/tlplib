using System;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class BoolExts {
    public static Option<T> opt<T>(this bool condition, Func<T> value) {
      return condition ? F.some(value()) : F.none<T>();
    }

    public static Option<T> opt<T>(this bool condition, T value) {
      return condition ? F.some(value) : F.none<T>();
    }

    public static Either<A, B> either<A, B>(
      this bool condition, Func<A> onFalse, Func<B> onRight
    ) { return !condition ? F.left<A, B>(onFalse()) : F.right<A, B>(onRight()); }

    public static Either<A, B> either<A, B>(
      this bool condition, A onFalse, Func<B> onRight
    ) { return !condition ? F.left<A, B>(onFalse) : F.right<A, B>(onRight()); }

    public static Either<A, B> either<A, B>(
      this bool condition, Func<A> onFalse, B onRight
    ) { return !condition ? F.left<A, B>(onFalse()) : F.right<A, B>(onRight); }

    public static Either<A, B> either<A, B>(
      this bool condition, A onFalse, B onTrue
    ) { return !condition ? F.left<A, B>(onFalse) : F.right<A, B>(onTrue); }

    public static int toInt(this bool b) => b ? 1 : 0;
  }
}
