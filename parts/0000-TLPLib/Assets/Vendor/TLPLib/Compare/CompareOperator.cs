using System;

namespace com.tinylabproductions.TLPLib.Compare {
  public enum Operator : byte { Less, LessOrEqual, Equals, GreaterOrEqual, Greater }

  public static class OperatorUtils {
    static bool compare<A>(A a, Operator op, A b) where A : IComparable<A> {
      switch (op) {
        case Operator.Less: return a.CompareTo(b) < 0;
        case Operator.LessOrEqual: return a.CompareTo(b) <= 0;
        case Operator.Equals: return a.CompareTo(b) == 0;
        case Operator.GreaterOrEqual: return a.CompareTo(b) >= 0;
        case Operator.Greater: return a.CompareTo(b) > 0;
        default: throw new ArgumentOutOfRangeException(nameof(op), op, null);
      }
    }

    public static bool compare<A>(this Operator op, A a, A b) where A : IComparable<A> => compare(a, op, b);
  }
}