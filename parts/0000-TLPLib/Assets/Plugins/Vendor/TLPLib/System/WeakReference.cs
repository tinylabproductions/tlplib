using System;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.system {
  public static class WeakReferenceTLP {
    public static WeakReferenceTLP<A> a<A>(
      A reference, bool trackResurrection = false
    ) where A : class =>
      new WeakReferenceTLP<A>(reference, trackResurrection);
  }

  public struct WeakReferenceTLP<A> where A : class {
    public readonly WeakReference<A> dotNet;

    public WeakReferenceTLP(A reference, bool trackResurrection = false) {
      dotNet = new WeakReference<A>(reference, trackResurrection);
    }

    public Option<A> Target => dotNet.TryGetTarget(out var a) ? F.some(a) : F.none_;

    public override string ToString() =>
      $"{nameof(WeakReferenceTLP<A>)}({Target})";
  }
}