using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Reactive {
  public enum RxSubscriptionMode : byte {
    ForSideEffects, ForRxMapping
  }

  static class RxBase {
    public static void set<A>(
      IEqualityComparer<A> comparer, ref A currentValue, A newValue, IObserver<A> observer
    ) {
      if (!comparer.Equals(currentValue, newValue)) {
        currentValue = newValue;
        observer.push(newValue);
      }
    }
  }
}