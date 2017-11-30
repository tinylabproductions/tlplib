using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
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
    public static int randomIndex<A>(this NonEmpty<ImmutableArray<A>> ne) => ne.a.randomIndex().get;
    public static A random<A>(this NonEmpty<ImmutableArray<A>> ne) => ne.a.random().get;
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
    public static Option<NonEmpty<A>> __unsafeApply(A c, Fn<A, bool> isEmpty) =>
      isEmpty(c) ? Option<NonEmpty<A>>.None : F.some(new NonEmpty<A>(c));

    public override string ToString() => $"{nameof(NonEmpty<A>)}({a})";

    public static implicit operator A(NonEmpty<A> ne) => ne.a;
  }
}