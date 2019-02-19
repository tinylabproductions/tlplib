using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class KeyValuePairExts {
    public static KeyValuePair<KK, VV> map<K, KK, V, VV>(
      this KeyValuePair<K, V> kv,
      Func<K, KK> keyMapper, Func<V, VV> valueMapper
    ) => new KeyValuePair<KK, VV>(keyMapper(kv.Key), valueMapper(kv.Value));

    public static KeyValuePair<KK, V> mapKey<K, KK, V>(
      this KeyValuePair<K, V>  kv, Func<K, KK> mapper
    ) => new KeyValuePair<KK, V>(mapper(kv.Key), kv.Value);

    public static KeyValuePair<K, VV> mapValue<K, V, VV>(
      this KeyValuePair<K, V>  kv, Func<V, VV> mapper
    ) => new KeyValuePair<K, VV>(kv.Key, mapper(kv.Value));
  }
}