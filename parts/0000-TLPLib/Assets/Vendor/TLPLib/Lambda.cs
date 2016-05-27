using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib {
  public static class Lambda {
    class Comparer<A> : IComparer<A> {
      readonly Fn<A, A, int> comparer;

      public Comparer(Fn<A, A, int> comparer) { this.comparer = comparer; }

      public int Compare(A x, A y) { return comparer(x, y); }
    }

    public static IComparer<A> comparer<A>(Fn<A, A, int> comparer) {
      return new Comparer<A>(comparer);
    }
  }
}
