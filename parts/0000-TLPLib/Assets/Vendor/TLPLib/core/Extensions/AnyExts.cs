using System;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class AnyExts {
    public static A orElseIfNull<A>(this A a, Func<A> ifNull) where A : class =>
      F.isNull(a) ? ifNull() : a;

    public static A orElseIfNull<A>(this A a, A ifNull) where A : class =>
      F.isNull(a) ? ifNull : a;
  }
}