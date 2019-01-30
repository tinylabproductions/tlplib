using System.Collections.Immutable;
using System.Linq;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class Rope {
    public static Rope<A> a<A>(A[] first) => new Rope<A>(first);
    public static Rope<A> a<A>(A[] first, A[] second) =>
      new Rope<A>(first, ImmutableList.Create<A[]>(second));
    public static Rope<A> create<A>(params A[] args) => new Rope<A>(args);
  }

  /**
   * Immutable data structure to efficiently string arrays together.
   */
  public struct Rope<A> {
    readonly A[] first;
    readonly ImmutableList<A[]> rest;

    public Rope(A[] first) : this(first, ImmutableList<A[]>.Empty) {}

    public Rope(A[] first, ImmutableList<A[]> rest) {
      this.first = first;
      this.rest = rest;
    }

    public static Rope<A> operator +(Rope<A> r1, Rope<A> r2) =>
      new Rope<A>(r1.first, r1.rest.Add(r2.first).AddRange(r2.rest));

    public static Rope<A> operator +(Rope<A> r1, A[] r2) =>
      new Rope<A>(r1.first, r1.rest.Add(r2));

    public int length => first.Length + rest.Sum(_ => _.Length);

    public ImmutableArray<A>.Builder builder() {
      var b = ImmutableArray.CreateBuilder<A>(length);
      b.AddRange(first);
      foreach (var arr in rest) b.AddRange(arr);
      return b;
    }

    public A[] toArray() => builder().ToArray();
    public ImmutableArray<A> toImmutable() => builder().MoveToImmutable();
  }
}