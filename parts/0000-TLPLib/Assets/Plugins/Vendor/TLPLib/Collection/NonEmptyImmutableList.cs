using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class NonEmptyImmutableList {
    public static Option<NonEmptyImmutableList<A>> a<A>(IEnumerable<A> enumerable) {
      var first = Option<A>.None;
      ImmutableList<A>.Builder restBuilder = null;
      foreach (var a in enumerable) {
        if (first.isNone) {
          first = F.some(a);
          restBuilder = ImmutableList.CreateBuilder<A>();
        }
        else {
          // ReSharper disable once PossibleNullReferenceException
          restBuilder.Add(a);
        }
      }
      // ReSharper disable once PossibleNullReferenceException
      return first.map(a => new NonEmptyImmutableList<A>(a, restBuilder.ToImmutable()));
    }
  }

  public class NonEmptyImmutableList<A> {
    public readonly A first;
    public readonly ImmutableList<A> rest;

    public NonEmptyImmutableList(A first, ImmutableList<A> rest) {
      this.first = first;
      this.rest = rest;
    }

    public int count => rest.Count + 1;
    public uint countUInt => (uint) rest.Count + 1;

    public A this[int idx] => idx == 0 ? first : rest[idx - 1];
    public A this[uint idx] => idx == 0 ? first : rest[idx.toIntClamped() - 1];
  }
}