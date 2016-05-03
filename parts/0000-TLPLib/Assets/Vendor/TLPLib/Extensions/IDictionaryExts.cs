using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IDictionaryExts {
    public static Option<V> get<K, V>(this IDictionary<K, V> dict, K key) {
      V outVal;
      return dict.TryGetValue(key, out outVal)
        ? F.some(outVal) : F.none<V>();
    }

    public static V getOrElse<K, V>(
      this IDictionary<K, V> dict, K key, Fn<V> orElse
    ) {
      V outVal;
      return dict.TryGetValue(key, out outVal) ? outVal : orElse();
    }

    // TODO: test
    public static V getOrUpdate<K, V>(
      this IDictionary<K, V> dict, K key, Fn<V> ifNotFound
    ) {
      return dict.getOrElse(key, () => {
        var v = ifNotFound();
        dict.Add(key, v);
        return v;
      });
    }
  }
}
