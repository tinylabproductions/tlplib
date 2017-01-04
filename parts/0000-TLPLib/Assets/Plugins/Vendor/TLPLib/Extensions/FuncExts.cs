using System;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FuncExts {
    public static Act<A> andThen<A, B>(this Fn<A, B> f, Act<B> a) => value => a(f(value));
    public static Fn<A, C> andThen<A, B, C>(this Fn<A, B> f, Fn<B, C> f1) => 
      value => f1(f(value));
  }
}
