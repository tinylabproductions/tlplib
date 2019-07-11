using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class ImmutableListExts {
    public static ImmutableList<A> Add<A>(
      this ImmutableList<A> list, Option<A> maybeA
    ) => maybeA.isSome ? list.Add(maybeA.__unsafeGetValue) : list;
  }
}