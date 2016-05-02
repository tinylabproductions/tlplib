using System;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FuncExts {
    public static Act<A> andThen<A, B>(this Fn<A, B> f, Act<B> a) {
      return value => a(f(value));
    }
  }
}
