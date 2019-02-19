using System;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FuncExts {
    public static Action<A> andThen<A, B>(this Func<A, B> f, Action<B> a) => value => a(f(value));
    public static Func<A, C> andThen<A, B, C>(this Func<A, B> f, Func<B, C> f1) =>
      value => f1(f(value));
  }
}
