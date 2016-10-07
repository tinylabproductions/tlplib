using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib {
  public enum CompareResult : sbyte { LT = -1, EQ = 0, GT = 1 }

  public static class CompareResultExts {
    public static CompareResult asCmpRes(this int result) =>
        result < 0 ? CompareResult.LT 
      : result == 0 ? CompareResult.EQ 
      : CompareResult.GT;
  }

  public static class Lambda {
    class Comparer<A> : IComparer<A> {
      readonly Fn<A, A, CompareResult> comparer;

      public Comparer(Fn<A, A, CompareResult> comparer) { this.comparer = comparer; }

      public int Compare(A x, A y) => (int) comparer(x, y);
    }

    public static IComparer<A> comparer<A>(Fn<A, A, CompareResult> comparer) => 
      new Comparer<A>(comparer);
  }
}
