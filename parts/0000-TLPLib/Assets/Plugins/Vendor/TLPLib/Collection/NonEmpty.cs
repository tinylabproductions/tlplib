using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class NonEmpty {
    public static Option<NonEmpty<ImmutableList<A>>> toNonEmpty<A>(this ImmutableList<A> c) =>
      NonEmpty<ImmutableList<A>>.__unsafeApply(c, _ => _.IsEmpty);

    public static Option<NonEmpty<ImmutableArray<A>>> toNonEmpty<A>(this ImmutableArray<A> c) =>
      NonEmpty<ImmutableArray<A>>.__unsafeApply(c, _ => _.IsEmpty);

    public static Option<NonEmpty<ImmutableHashSet<A>>> toNonEmpty<A>(this ImmutableHashSet<A> c) =>
      NonEmpty<ImmutableHashSet<A>>.__unsafeApply(c, _ => _.IsEmpty);

    public static Option<NonEmpty<ImmutableSortedSet<A>>> toNonEmpty<A>(this ImmutableSortedSet<A> c) =>
      NonEmpty<ImmutableSortedSet<A>>.__unsafeApply(c, _ => _.IsEmpty);
  }

  public static class NonEmptyExts {
    public static A head<A>(this NonEmpty<ImmutableList<A>> ne) => ne.a[0];
    public static A head<A>(this NonEmpty<ImmutableArray<A>> ne) => ne.a[0];

    public static NonEmpty<IEnumerable<B>> map<A, B, C>(
      this NonEmpty<C> ne, Func<A, B> f
    ) where C : IEnumerable<A> =>
      NonEmpty<IEnumerable<B>>.__unsafeNew(ne.a.Select(f));

    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableArray<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableArray<A>>(ne, f);

    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableList<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableList<A>>(ne, f);

    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableHashSet<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableHashSet<A>>(ne, f);

    public static NonEmpty<IEnumerable<B>> map<A, B>(this NonEmpty<ImmutableSortedSet<A>> ne, Func<A, B> f) =>
      map<A, B, ImmutableSortedSet<A>>(ne, f);

    public static NonEmpty<ImmutableArray<A>> ToImmutableArray<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableArray<A>>.__unsafeNew(ne.a.ToImmutableArray());

    public static NonEmpty<ImmutableList<A>> ToImmutableList<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableList<A>>.__unsafeNew(ne.a.ToImmutableList());

    public static NonEmpty<ImmutableHashSet<A>> ToImmutableHashSet<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableHashSet<A>>.__unsafeNew(ne.a.ToImmutableHashSet());

    public static NonEmpty<ImmutableSortedSet<A>> ToImmutableSortedSet<A>(this NonEmpty<IEnumerable<A>> ne) =>
      NonEmpty<ImmutableSortedSet<A>>.__unsafeNew(ne.a.ToImmutableSortedSet());
  }

  public struct NonEmpty<A> : IEquatable<NonEmpty<A>> {
    public readonly A a;

    #region Equality

    public bool Equals(NonEmpty<A> other) {
      return EqualityComparer<A>.Default.Equals(a, other.a);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is NonEmpty<A> && Equals((NonEmpty<A>) obj);
    }

    public override int GetHashCode() {
      return EqualityComparer<A>.Default.GetHashCode(a);
    }

    public static bool operator ==(NonEmpty<A> left, NonEmpty<A> right) { return left.Equals(right); }
    public static bool operator !=(NonEmpty<A> left, NonEmpty<A> right) { return !left.Equals(right); }

    #endregion

    NonEmpty(A a) { this.a = a; }

    /// <summary>You should never use this method directly.</summary>
    internal static Option<NonEmpty<A>> __unsafeApply(A c, Fn<A, bool> isEmpty) =>
      isEmpty(c) ? Option<NonEmpty<A>>.None : F.some(__unsafeNew(c));

    /// <summary>Never use this from user code, this is only intended for library use.</summary>
    internal static NonEmpty<A> __unsafeNew(A c) => new NonEmpty<A>(c);

    public override string ToString() => $"{nameof(NonEmpty<A>)}({a})";

    public static implicit operator A(NonEmpty<A> ne) => ne.a;
  }
}