using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Reactive {
  static class RxValCache<A> {
    static readonly Dictionary<A, RxValStatic<A>> staticCache = 
      new Dictionary<A, RxValStatic<A>>();

    public static IRxVal<A> get(A value) => 
      staticCache.getOrUpdate(value, () => RxValStatic.a(value));
  }
}