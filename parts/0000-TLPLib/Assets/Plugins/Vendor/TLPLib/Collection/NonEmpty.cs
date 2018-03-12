using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class NonEmpty {
    [PublicAPI]
    public static Option<NonEmpty<ImmutableList<A>>> toNonEmpty<A>(this ImmutableList<A> c) =>
      NonEmpty<ImmutableList<A>>.__unsafeApply(c, _ => _.IsEmpty);

    [PublicAPI] 
    public static Option<NonEmpty<ImmutableArray<A>>> toNonEmpty<A>(this ImmutableArray<A> c) =>
      NonEmpty<ImmutableArray<A>>.__unsafeApply(c, _ => _.IsEmpty);

    [PublicAPI] 
    public static Option<NonEmpty<A[]>> toNonEmpty<A>(this A[] c) =>
      NonEmpty<A[]>.__unsafeApply(c, _ => _.isEmpty());

    [PublicAPI] 
    public static Option<NonEmpty<ImmutableHashSet<A>>> toNonEmpty<A>(this ImmutableHashSet<A> c) =>
      NonEmpty<ImmutableHashSet<A>>.__unsafeApply(c, _ => _.IsEmpty);

    [PublicAPI] 
    public static Option<NonEmpty<ImmutableSortedSet<A>>> toNonEmpty<A>(this ImmutableSortedSet<A> c) =>
      NonEmpty<ImmutableSortedSet<A>>.__unsafeApply(c, _ => _.IsEmpty);

    [PublicAPI] 
    public static NonEmpty<ImmutableArray<A>> array<A>(A a1) =>
      NonEmpty<ImmutableArray<A>>.__unsafeNew(ImmutableArray.Create(a1));

    [PublicAPI] 
    public static NonEmpty<ImmutableArray<A>> array<A>(A a1, A a2) =>
      NonEmpty<ImmutableArray<A>>.__unsafeNew(ImmutableArray.Create(a1, a2));

    [PublicAPI] 
    public static NonEmpty<ImmutableArray<A>> array<A>(A a1, A a2, A a3) =>
      NonEmpty<ImmutableArray<A>>.__unsafeNew(ImmutableArray.Create(a1, a2, a3));

    [PublicAPI] 
    public static NonEmpty<ImmutableArray<A>> array<A>(A a1, A a2, A a3, A a4) =>
      NonEmpty<ImmutableArray<A>>.__unsafeNew(ImmutableArray.Create(a1, a2, a3, a4));

    [PublicAPI] 
    public static NonEmpty<ImmutableArray<A>> array<A>(A first, params A[] rest) {
      var b = ImmutableArray.CreateBuilder<A>(rest.Length + 1);
      b.Add(first);
      b.AddRange(rest);
      return NonEmpty<ImmutableArray<A>>.__unsafeNew(b.MoveToImmutable());
    }
  }

  public static class NonEmptyExts {
    [PublicAPI] public static A head<A>(this NonEmpty<ImmutableList<A>> ne) => ne.a[0];
    [PublicAPI] public static A head<A>(this NonEmpty<ImmutableArray<A>> ne) => ne.a[0];
    [PublicAPI] public static A head<A>(this NonEmpty<A[]> ne) => ne.a[0];

    [PublicAPI]
    public static NonEmpty<IEnumerable<B>> map<A, B, C>(
      this NonEmpty<C> ne, Func<A, B> f
    ) where C : IEnumerable<A> =>
      NonEmpty<IEnumerable<B>>.__unsafeNew(ne.a.Select(f));

    [PublicAPI]
    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableArray<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableArray<A>>(ne, f);

    [PublicAPI]
    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableList<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableList<A>>(ne, f);

    [PublicAPI]
    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableHashSet<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableHashSet<A>>(ne, f);

    [PublicAPI]
    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableSortedSet<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableSortedSet<A>>(ne, f);

    [PublicAPI]
    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<A[]> ne, Func<A, B> f) =>
      map<A, B, A[]>(ne, f);

    [PublicAPI]
    public static NonEmpty<ImmutableArray<A>> ToImmutableArray<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableArray<A>>.__unsafeNew(ne.a.ToImmutableArray());

    [PublicAPI]
    public static NonEmpty<ImmutableList<A>> ToImmutableList<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableList<A>>.__unsafeNew(ne.a.ToImmutableList());

    [PublicAPI]
    public static NonEmpty<ImmutableHashSet<A>> ToImmutableHashSet<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableHashSet<A>>.__unsafeNew(ne.a.ToImmutableHashSet());

    [PublicAPI]
    public static NonEmpty<ImmutableSortedSet<A>> ToImmutableSortedSet<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableSortedSet<A>>.__unsafeNew(ne.a.ToImmutableSortedSet());
  }

  [Record(GenerateToString = false)]
  public partial struct NonEmpty<A> {
    public readonly A a;

    /// <summary>You should never use this method directly.</summary>
    internal static Option<NonEmpty<A>> __unsafeApply(A c, Fn<A, bool> isEmpty) =>
      isEmpty(c) ? Option<NonEmpty<A>>.None : F.some(__unsafeNew(c));

    /// <summary>Never use this from user code, this is only intended for library use.</summary>
    internal static NonEmpty<A> __unsafeNew(A c) => new NonEmpty<A>(c);

    public override string ToString() => $"{nameof(NonEmpty<A>)}({a})";

    public static implicit operator A(NonEmpty<A> ne) => ne.a;
  }
}