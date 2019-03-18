using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.reflection;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ImmutableArrayUnsafe {
    public static ImmutableArray<A> createByMove<A>(A[] arr) =>
      ImmutableArrayUnsafe<A>.constructor(new object[] { arr });

    public static A[] internalArray<A>(this ImmutableArray<A> arr) =>
      ImmutableArrayUnsafe<A>.internalArray(arr);
  }

  public static class ImmutableArrayUnsafe<A> {
    public static readonly Func<object[], ImmutableArray<A>> constructor =
      PrivateConstructor.creator<ImmutableArray<A>>();

    public static readonly Func<ImmutableArray<A>, A[]> internalArray =
      PrivateField.getter<ImmutableArray<A>, A[]>("array");
  }
}