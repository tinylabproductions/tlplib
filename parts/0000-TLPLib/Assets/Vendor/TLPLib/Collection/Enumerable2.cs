using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Collection {
  /* As System.Enumerable. */
  [PublicAPI] public static class Enumerable2 {
    public static IEnumerable<A> fromImperative<A>(int count, Func<int, A> get) {
      for (var idx = 0; idx < count; idx++)
        yield return get(idx);
    }

    /// <summary>Enumerable from starting number to int.MaxValue</summary>
    public static IEnumerable<int> from(int startingNumber) {
      return Enumerable.Range(startingNumber, int.MaxValue - startingNumber);
    }

    public static IEnumerable<A> fill<A>(int count, Func<A> create) {
      for (var idx = 0; idx < count; idx++)
        yield return create();
    }

    /// <summary>
    /// Given a total number, distribute it over X slots.
    /// </summary>
    /// <param name="total"></param>
    /// <param name="slots"></param>
    /// <returns></returns>
    public static uint[] distribution(uint total, uint slots) {
      if (slots == 0) return new uint[0];
      
      var dist = new uint[slots];
      var perSlot = total / slots;
      var remainder = total - perSlot * slots; 
      dist.fill(perSlot);
      var idx = 0;
      while (remainder > 0) {
        dist[idx % dist.Length]++;
        remainder--;
        idx++;
      }
      return dist;
    }

    public static IEnumerable<A> yield<A>(A a1, A a2) {
      yield return a1;
      yield return a2;
    }
  }
}
