using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxValCache<A> {
    static readonly Dictionary<A, IRxVal<A>> staticCache = new Dictionary<A, IRxVal<A>>();

    public static IRxVal<A> get(A value) {
      IRxVal<A> rxVal;
      if (!staticCache.TryGetValue(value, out rxVal)) {
        rxVal = RxValStatic.a(value);
        staticCache.Add(value, rxVal);
      }
      return rxVal;
    }
  }
}