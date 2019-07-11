using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxBase {
    public static bool compareAndSet<A>(
      IEqualityComparer<A> comparer, ref A oldValue, A newValue
    ) {
      if (!comparer.Equals(oldValue, newValue)) {
        oldValue = newValue;
        return true;
      }

      return false;
    }
  }
}