using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  [PublicAPI] public enum CompareResult : sbyte { LT = -1, EQ = 0, GT = 1 }

  [PublicAPI] public static class CompareResultExts {
    public static int asInt(this CompareResult res) => (int) res;

    public static CompareResult asCmpRes(this int result) =>
        result < 0 ? CompareResult.LT
      : result == 0 ? CompareResult.EQ
      : CompareResult.GT;
  }

  [PublicAPI] public interface Comparable<A> : IComparer<A>, Eql<A> {
    CompareResult compare(A a1, A a2);
  }

  [PublicAPI] public static class Comparable {
    public static readonly Comparable<int> integer = lambda<int>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<uint> uint_ = lambda<uint>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<long> long_ = lambda<long>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<short> short_ = lambda<short>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<ushort> ushort_ = lambda<ushort>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<ulong> ulong_ = lambda<ulong>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<float> float_ = lambda<float>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<double> double_ = lambda<double>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<bool> bool_ = lambda<bool>((a1, a2) => a1.CompareTo(a2));
    public static readonly Comparable<string> string_ =
      // ReSharper disable once ConvertClosureToMethodGroup - the call is ambiguous
      lambda<string>((s1, s2) => string.CompareOrdinal(s1, s2));

    public static Comparable<A> lambda<A>(Func<A, A, CompareResult> compare) =>
      new Lambda<A>(compare);
    public static Comparable<A> lambda<A>(Func<A, A, int> compare) =>
      new Lambda<A>((a1, a2) => compare(a1, a2).asCmpRes());

    public static Comparable<A> inverse<A>(this Comparable<A> cmp) =>
      lambda<A>((a1, a2) => {
        switch (cmp.compare(a1, a2)) {
          case CompareResult.LT: return CompareResult.GT;
          case CompareResult.EQ: return CompareResult.EQ;
          case CompareResult.GT: return CompareResult.LT;
          default: throw new ArgumentOutOfRangeException();
        }
      });

    public static Comparable<A> and<A>(this Comparable<A> a1Cmp, Comparable<A> a2Cmp) => lambda<A>(
      (a1, a2) => {
        var aRes = a1Cmp.compare(a1, a2);
        return aRes == CompareResult.EQ ? a2Cmp.compare(a1, a2) : aRes;
      }
    );

    public static Comparable<Tpl<A, B>> tpl<A, B>(Comparable<A> aCmp, Comparable<B> bCmp) => lambda<Tpl<A, B>>(
      (t1, t2) => {
        var (a1, b1) = t1;
        var (a2, b2) = t2;
        var aRes = aCmp.compare(a1, a2);
        return aRes == CompareResult.EQ ? bCmp.compare(b1, b2) : aRes;
      }
    );
    
    public static Comparable<Tpl<A, B, C>> tpl<A, B, C>(
      Comparable<A> aCmp, Comparable<B> bCmp, Comparable<C> cCmp
    ) => lambda<Tpl<A, B, C>>(
      (t1, t2) => {
        var (a1, b1, c1) = t1;
        var (a2, b2, c2) = t2;
        var aRes = aCmp.compare(a1, a2);
        if (aRes != CompareResult.EQ) return aRes;
        var bRes = bCmp.compare(b1, b2);
        if (bRes != CompareResult.EQ) return bRes;
        return cCmp.compare(c1, c2);
      }
    );
    
    public static Comparable<Tpl<A, B, C, D>> tpl<A, B, C, D>(
      Comparable<A> aCmp, Comparable<B> bCmp, Comparable<C> cCmp, Comparable<D> dCmp
    ) => lambda<Tpl<A, B, C, D>>(
      (t1, t2) => {
        var (a1, b1, c1, d1) = t1;
        var (a2, b2, c2, d2) = t2;
        var aRes = aCmp.compare(a1, a2);
        if (aRes != CompareResult.EQ) return aRes;
        var bRes = bCmp.compare(b1, b2);
        if (bRes != CompareResult.EQ) return bRes;
        var cRes = cCmp.compare(c1, c2);
        if (cRes != CompareResult.EQ) return cRes;
        return dCmp.compare(d1, d2);
      }
    );

    public static Comparable<A> by<A, B>(Func<A, B> mapper, Comparable<B> cmp) => lambda<A>((a1, a2) => {
      var b1 = mapper(a1);
      var b2 = mapper(a2);
      return cmp.compare(b1, b2);
    });

    class Lambda<A> : Comparable<A> {
      readonly Func<A, A, CompareResult> _compare;

      public Lambda(Func<A, A, CompareResult> compare) { _compare = compare; }

      public CompareResult compare(A a1, A a2) => _compare(a1, a2);
      public bool eql(A a1, A a2) => _compare(a1, a2) == CompareResult.EQ;
      public int Compare(A x, A y) => (int) _compare(x, y);
    }
  }

  public static class ComparableOps {
    public static Comparable<B> comap<A, B>(this Comparable<A> cmp, Func<B, A> mapper) =>
      Comparable.lambda<B>((b1, b2) => cmp.compare(mapper(b1), mapper(b2)));

    public static bool lt<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) == CompareResult.LT;
    public static bool lte<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) != CompareResult.GT;
    public static bool eq<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) == CompareResult.EQ;
    public static bool gt<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) == CompareResult.GT;
    public static bool gte<A>(this Comparable<A> cmp, A a1, A a2) => cmp.compare(a1, a2) != CompareResult.LT;
    public static A min<A>(this Comparable<A> cmp, A a1, A a2) => cmp.lt(a1, a2) ? a1 : a2;
    public static A min<A>(this A a1, A a2, Comparable<A> cmp) => cmp.min(a1, a2);
    public static A max<A>(this Comparable<A> cmp, A a1, A a2) => cmp.gt(a1, a2) ? a1 : a2;
    public static A max<A>(this A a1, A a2, Comparable<A> cmp) => cmp.max(a1, a2);

    public static Option<A> max<A, Coll>(
      this Coll c, Comparable<A> comparable
    ) where Coll : IEnumerable<A> => maxBy<A, A, Coll>(c, comparable, _ => _);

    public static Option<A> maxBy<A, B, Coll>(
      this Coll c, Comparable<B> comparable, Func<A, B> extract
    ) where Coll : IEnumerable<A> => minMax(c, extract, comparable, CompareResult.GT);

    public static Option<A> maxBy<A, B>(
      this IEnumerable<A> c, Comparable<B> comparable, Func<A, B> extract
    ) => maxBy<A, B, IEnumerable<A>>(c, comparable, extract);

    public static Option<A> maxBy<A>(
      this IEnumerable<A> c, Comparable<A> comparable
    ) => c.maxBy(comparable, _ => _);

    public static Option<A> min<A, Coll>(
      this Coll c, Comparable<A> comparable
    ) where Coll : IEnumerable<A> => minBy<A, A, Coll>(c, comparable, _ => _);

    public static Option<A> minBy<A, B, Coll>(
      this Coll c, Comparable<B> comparable, Func<A, B> extract
    ) where Coll : IEnumerable<A> => minMax(c, extract, comparable, CompareResult.LT);

    public static Option<A> minBy<A, B>(
      this IEnumerable<A> c, Comparable<B> comparable, Func<A, B> extract
    ) => minBy<A, B, IEnumerable<A>>(c, comparable, extract);

    public static Option<A> minBy<A>(
      this IEnumerable<A> c, Comparable<A> comparable
    ) => c.minBy(comparable, _ => _);

    static Option<A> minMax<A, B, Coll>(
      this Coll c, Func<A, B> extract, Comparable<B> comparable, CompareResult lookFor
    ) where Coll : IEnumerable<A> {
      var current = Option<A>.None;
      foreach (var a in c) {
        if (current.isNone) current = a.some();
        else {
          if (comparable.compare(extract(a), extract(current.get)) == lookFor)
            current = a.some();
        }
      }
      return current;
    }
  }
}