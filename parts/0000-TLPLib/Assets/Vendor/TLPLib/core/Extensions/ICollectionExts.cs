using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.data;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI]
  public static class ICollectionExts {
    
    public static B[] ToArray<A, B>(this ICollection<A> collection, Func<A, B> mapper) {
      var bArr = new B[collection.Count];
      var idx = 0;
      foreach (var a in collection) {
        bArr[idx] = mapper(a);
        idx++;
      }
      return bArr;
    }

    public static Range indexRange<A>(this ICollection<A> coll) =>
      new Range(0, coll.Count - 1);

    /// <summary>
    /// Given a collection of tuples (A, B), shuffle their Bs.
    ///
    /// For example:
    /// <code><![CDATA[
    /// Given           : [(1, '1'), (2, '2'), (3, '3')]
    /// (one of) results: [(1, '3'), (2, '1'), (3, '2')]
    /// ]]></code>
    /// </summary>
    public static ImmutableList<TupleType> shuffleTuplePairs<TupleType, A, B>(
      this ICollection<TupleType> tuples, ref Rng rng,
      Func<TupleType, A> extractFirst, Func<TupleType, B> extractSecond,
      Func<A, B, TupleType> createTuple
    ) {
      var r = rng;
      var result =
        tuples.Select(extractFirst)
        .zip(
          tuples.Select(extractSecond).OrderBySafe(_ => r.nextInt(out r)),
          createTuple
        )
        .ToImmutableList(); // Force to update rng.
      rng = r;
      return result;
    }
    
    public static IEnumerable<A> shuffleRepeatedly<A>(this ICollection<A> collection, Rng rng) {
      var copy = collection.ToList();
      while (true) {
        copy.shuffle(ref rng);
        foreach (var item in copy) {
          yield return item;
        }
      }
    }

    public static Option<double> average<A>(this ICollection<A> collection, Func<A, double> mapper) {
      if (collection.Count == 0) return F.none_;
      double result = 0;
      foreach (var item in collection) {
        result += mapper(item);
      }
      return (result / collection.Count).some();
    }
  }
}