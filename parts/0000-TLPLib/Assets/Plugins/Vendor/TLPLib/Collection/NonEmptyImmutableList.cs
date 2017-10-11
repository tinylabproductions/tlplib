using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class NonEmptyImmutableList {
    public static Option<NonEmptyImmutableList<A>> a<A>(ImmutableList<A> list) => NonEmptyImmutableList<A>.a(list);
    public static Option<NonEmptyImmutableList<A>> toNonEmpty<A>(this ImmutableList<A> list) => a(list);
  }

  public struct NonEmptyImmutableList<A> : IEquatable<NonEmptyImmutableList<A>> {
    public readonly ImmutableList<A> list;

    #region Equality

    public bool Equals(NonEmptyImmutableList<A> other) {
      return Equals(list, other.list);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is NonEmptyImmutableList<A> && Equals((NonEmptyImmutableList<A>)obj);
    }

    public override int GetHashCode() {
      return (list != null ? list.GetHashCode() : 0);
    }

    public static bool operator ==(NonEmptyImmutableList<A> left, NonEmptyImmutableList<A> right) { return left.Equals(right); }
    public static bool operator !=(NonEmptyImmutableList<A> left, NonEmptyImmutableList<A> right) { return !left.Equals(right); }

    #endregion

    NonEmptyImmutableList(ImmutableList<A> list) { this.list = list; }
    
    public static Option<NonEmptyImmutableList<A>> a(ImmutableList<A> list) =>
      list.isEmpty() ? Option<NonEmptyImmutableList<A>>.None : F.some(new NonEmptyImmutableList<A>(list));

    public override string ToString() => $"{nameof(NonEmptyImmutableList<A>)}({list.mkStringEnum()})";
  }
}