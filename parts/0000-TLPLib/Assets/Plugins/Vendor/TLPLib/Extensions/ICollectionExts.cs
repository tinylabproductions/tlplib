using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ICollectionExts {
    public static B[] ToArray<A, B>(this ICollection<A> collection, Fn<A, B> mapper) {
      var bArr = new B[collection.Count];
      var idx = 0;
      foreach (var a in collection) {
        bArr[idx] = mapper(a);
        idx++;
      }
      return bArr;
    }

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
      Fn<A, B, TupleType> createTuple
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
  }
}