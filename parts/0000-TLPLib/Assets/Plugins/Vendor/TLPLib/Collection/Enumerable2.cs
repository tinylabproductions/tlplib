using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Collection {
  /* As System.Enumerable. */
  public static class Enumerable2 {
    [PublicAPI]
    public static IEnumerable<A> fromImperative<A>(int count, Fn<int, A> get) {
      for (var idx = 0; idx < count; idx++)
        yield return get(idx);
    }

    /// <summary>Enumerable from starting number to int.MaxValue</summary>
    [PublicAPI]
    public static IEnumerable<int> from(int startingNumber) {
      return Enumerable.Range(startingNumber, int.MaxValue - startingNumber);
    }

    [PublicAPI]
    public static IEnumerable<A> fill<A>(int count, Fn<A> create) {
      for (var idx = 0; idx < count; idx++)
        yield return create();
    }

    /// <summary>
    /// Given a total number, distribute it over X slots.
    /// </summary>
    /// <param name="total"></param>
    /// <param name="slots"></param>
    /// <returns></returns>
    [PublicAPI]
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
  }
}
