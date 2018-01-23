using WR = System.WeakReference;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.system {
  public static class WeakReference {
    public static WeakReference<A> a<A>(
      A reference, bool trackResurrection = false
    ) where A : class =>
      new WeakReference<A>(reference, trackResurrection);
  }

  public struct WeakReference<A> where A : class {
    public readonly WR untyped;

    public WeakReference(A reference, bool trackResurrection = false) {
      untyped = new WR(reference, trackResurrection);
    }

    public bool IsAlive => untyped.IsAlive;
    public bool TrackResurrection => untyped.TrackResurrection;
    public Option<A> Target => F.opt(untyped.Target as A);

    public override string ToString() =>
      $"{nameof(WeakReference<A>)}({Target})";
  }
}