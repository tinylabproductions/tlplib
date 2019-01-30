using System.Collections.Immutable;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ImmutableHashSetExts {
    public static ImmutableHashSet<A> toggle<A>(this ImmutableHashSet<A> hs, A a) =>
      hs.Contains(a) ? hs.Remove(a) : hs.Add(a);
  }
}