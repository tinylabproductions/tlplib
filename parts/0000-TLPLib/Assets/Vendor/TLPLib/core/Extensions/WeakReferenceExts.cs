using System;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class WeakReferenceExts {
    public static Option<A> Target<A>(this WeakReference<A> wr) where A : class =>
      wr.TryGetTarget(out var a) ? F.some(a) : F.none_;
  }
}