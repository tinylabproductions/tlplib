using System;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IComparableExts {
    public static bool lt<A>(this A a1, A a2) where A : IComparable<A> =>
      a1.CompareTo(a2) < 0;

    public static bool lte<A>(this A a1, A a2) where A : IComparable<A> =>
      a1.CompareTo(a2) <= 0;

    public static bool eq<A>(this A a1, A a2) where A : IComparable<A> =>
      a1.CompareTo(a2) == 0;

    public static bool gte<A>(this A a1, A a2) where A : IComparable<A> =>
      a1.CompareTo(a2) >= 0;

    public static bool gt<A>(this A a1, A a2) where A : IComparable<A> =>
      a1.CompareTo(a2) > 0;
  }
}