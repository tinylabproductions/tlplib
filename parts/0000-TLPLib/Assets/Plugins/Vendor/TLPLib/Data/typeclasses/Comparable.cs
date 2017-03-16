using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public interface Comparable<in A> {
    CompareResult compare(A a1, A a2);
  }

  public static class Comparable {
    public static readonly Comparable<int> integer = lambda<int>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<uint> uint_ = lambda<uint>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<long> long_ = lambda<long>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<ulong> ulong_ = lambda<ulong>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<float> float_ = lambda<float>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<double> double_ = lambda<double>((a1, a2) => a1.CompareTo(a2));

    public static Comparable<A> lambda<A>(Fn<A, A, CompareResult> compare) =>
      new Lambda<A>(compare);
    public static Comparable<A> lambda<A>(Fn<A, A, int> compare) =>
      new Lambda<A>((a1, a2) => compare(a1, a2).asCmpRes());

    class Lambda<A> : Comparable<A> {
      readonly Fn<A, A, CompareResult> _compare;

      public Lambda(Fn<A, A, CompareResult> compare) { _compare = compare; }

      public CompareResult compare(A a1, A a2) => _compare(a1, a2);
    }
  }

  public static class ComparableOps {
    public static Comparable<B> comap<A, B>(this Comparable<A> cmp, Fn<B, A> mapper) =>
      Comparable.lambda<B>((b1, b2) => cmp.compare(mapper(b1), mapper(b2)));

    public static bool lt<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) == CompareResult.LT;
    public static bool lte<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) != CompareResult.GT;
    public static bool eq<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) == CompareResult.EQ;
    public static bool gt<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) == CompareResult.GT;
    public static bool gte<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) != CompareResult.LT;
    public static A min<A>(this Comparable<A> cmp, A a1, A a2) => cmp.lt(a1, a2) ? a1 : a2;
    public static A max<A>(this Comparable<A> cmp, A a1, A a2) => cmp.gt(a1, a2) ? a1 : a2;

    public static Option<A> max<A, Coll>(
      this Coll c, Comparable<A> comparable
    ) where Coll : IEnumerable<A> => maxBy<A, A, Coll>(c, comparable, _ => _);

    public static Option<A> maxBy<A, B, Coll>(
      this Coll c, Comparable<B> comparable, Fn<A, B> extract
    ) where Coll : IEnumerable<A> => minMax(c, extract, comparable, CompareResult.GT);

    public static Option<A> maxBy<A, B>(
      this IEnumerable<A> c, Comparable<B> comparable, Fn<A, B> extract
    ) => maxBy<A, B, IEnumerable<A>>(c, comparable, extract);

    public static Option<A> min<A, Coll>(
      this Coll c, Comparable<A> comparable
    ) where Coll : IEnumerable<A> => minBy<A, A, Coll>(c, comparable, _ => _);

    public static Option<A> minBy<A, B, Coll>(
      this Coll c, Comparable<B> comparable, Fn<A, B> extract
    ) where Coll : IEnumerable<A> => minMax(c, extract, comparable, CompareResult.LT);

    public static Option<A> minBy<A, B>(
      this IEnumerable<A> c, Comparable<B> comparable, Fn<A, B> extract
    ) => minBy<A, B, IEnumerable<A>>(c, comparable, extract);

    static Option<A> minMax<A, B, Coll>(
      this Coll c, Fn<A, B> extract, Comparable<B> comparable, CompareResult lookFor
    ) where Coll : IEnumerable<A> {
      var current = Option<A>.None;
      foreach (var a in c) {
        if (current.isEmpty) current = a.some();
        else {
          if (comparable.compare(extract(a), extract(current.get)) == lookFor)
            current = a.some();
        }
      }
      return current;
    }
  }
}