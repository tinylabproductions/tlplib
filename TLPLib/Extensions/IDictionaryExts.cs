using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IDictionaryExts {
    public static Option<V> get<K, V>(this IDictionary<K, V> dict, K key) {
      V outVal;
      return dict.TryGetValue(key, out outVal)
        ? F.some(outVal) : F.none<V>();
    }

    /* as #[], but has a better error message */
    public static V a<K, V>(this IDictionary<K, V> dict, K key) {
      return dict.get(key).fold(
        () => { throw new KeyNotFoundException(string.Format(
          "Cannot find {0} in {1}", key.debugObj(), dict.asString()
        )); },
        _ => _
      );
    }

    public static V getOrElse<K, V>(
      this IDictionary<K, V> dict, K key, Fn<V> orElse
    ) {
      V outVal;
      return dict.TryGetValue(key, out outVal) ? outVal : orElse();
    }
  }
}
