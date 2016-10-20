using System;
using System.Collections.Generic;
using System.Linq;

namespace com.tinylabproductions.TLPLib.Collection {
  /* As System.Enumerable. */
  public static class Enumerable2 {
    public static IEnumerable<A> fromImperative<A>(int count, Fn<int, A> get) {
      for (var idx = 0; idx < count; idx++)
        yield return get(idx);
    }

    /* Enumerable from starting number to int.MaxValue */
    public static IEnumerable<int> from(int startingNumber) {
      return Enumerable.Range(startingNumber, int.MaxValue - startingNumber);
    }
  }
}
