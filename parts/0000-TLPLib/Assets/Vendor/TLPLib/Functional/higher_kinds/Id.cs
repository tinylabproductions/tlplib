using GenerationAttributes;
// ReSharper disable ClassNeverInstantiated.Global

namespace com.tinylabproductions.TLPLib.Functional.higher_kinds {
  public static class Id {
    public struct W {}

    public static Id<A> a<A>(A a) => new Id<A>(a);
    public static Id<A> narrowK<A>(this HigherKind<W, A> hkt) => (Id<A>) hkt;
  }
  /// <summary>Id monad is a way to lift a value into a monad when dealing with higher kinded code.</summary>
  [Record]
  public partial struct Id<A> : HigherKind<Id.W, A> {
    public readonly A a;

    public static implicit operator A(Id<A> id) => id.a;
    public static implicit operator Id<A>(A a) => new Id<A>(a);
  }
}