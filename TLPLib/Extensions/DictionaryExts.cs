using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class DictionaryExts {
    public static Option<B> tryGetOpt<A, B>(this Dictionary<A, B> dict, A key) {
      B val;
      return dict.TryGetValue(key, out val) ? val.some() : F.none<B>();
    }
  }
}
