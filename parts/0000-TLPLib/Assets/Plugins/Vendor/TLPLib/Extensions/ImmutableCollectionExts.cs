using System.Collections.Generic;
using System.Collections.Immutable;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ImmutableCollectionExts {
    public static ImmutableHashSet<A> addAll<A>(
      this ImmutableHashSet<A> set, IEnumerable<A> enumerable
    ) {
      foreach (var a in enumerable) set = set.Add(a);
      return set;
    }
  }
}
