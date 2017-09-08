using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ICollectionExts {
    public static B[] ToArray<A, B>(this ICollection<A> collection, Fn<A, B> mapper) {
      var bArr = new B[collection.Count];
      var idx = 0;
      foreach (var a in collection) {
        bArr[idx] = mapper(a);
        idx++;
      }
      return bArr;
    }
  }
}