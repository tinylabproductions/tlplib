﻿using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IDictionaryExts {
    public static Option<V> get<K, V>(this IDictionary<K, V> dict, K key) {
      V outVal;
      return dict.TryGetValue(key, out outVal) ? F.some(outVal) : F.none<V>();
    }

    public static Option<V> getAndRemove<K, V>(this IDictionary<K, V> dict, K key) {
      var opt = dict.get(key);
      if (opt.isSome) dict.Remove(key);
      return opt;
    }

    public static Option<V> get<K, V>(this IReadOnlyDictionary<K, V> dict, K key) {
      V outVal;
      return dict.TryGetValue(key, out outVal) ? F.some(outVal) : F.none<V>();
    }

    public static Either<string, V> getE<K, V>(this IDictionary<K, V> dict, K key) =>
      dict.get(key).toRight($"Can't find '{key}'!");

    public static V getOrUpdate<K, V>(
      this IDictionary<K, V> dict, K key, Fn<V> ifNotFound
    ) {
      return dict.getOrElse(key, () => {
        var v = ifNotFound();
        dict.Add(key, v);
        return v;
      });
    }

    public static V getOrElse<K, V>(
      this IDictionary<K, V> dict, K key, Fn<V> orElse
    ) {
      V outVal;
      return dict.TryGetValue(key, out outVal) ? outVal : orElse();
    }

    public static V getOrElse<K, V>(
      this IDictionary<K, V> dict, K key, V orElse
    ) {
      V outVal;
      return dict.TryGetValue(key, out outVal) ? outVal : orElse;
    }

    /* as #[], but has a better error message */
    public static V a<K, V>(this IDictionary<K, V> dict, K key) {
      foreach (var val in dict.get(key)) return val;
      throw new KeyNotFoundException($"Cannot find {key} in {dict.asDebugString()}");
    }

    public static bool isEmpty<K, V>(this IDictionary<K, V> dict) => dict.Count == 0;
    public static bool nonEmpty<K, V>(this IDictionary<K, V> dict) => dict.Count != 0;

    public static IDictionary<K, V> addAnd<K, V>(this IDictionary<K, V> dict, K key, V value) 
      => dict.addAnd(new KeyValuePair<K, V>(key, value));

    public static IDictionary<K, V> addAnd<K, V>(this IDictionary<K, V> dict, KeyValuePair<K, V> pair) {
      dict.Add(pair);
      return dict;
    }

    public static IDictionary<K, V> addOptAnd<K, V>(
      this IDictionary<K, V> dict, K key, Option<V> valueOpt
    ) {
      foreach (var value in valueOpt) dict.Add(key, value);
      return dict;
    }

    public static IDictionary<K, V> addAll<K, V>(
      this IDictionary<K, V> dict, IEnumerable<KeyValuePair<K, V>> enumerable
    ) {
      foreach (var kv in enumerable)
        dict.Add(kv);
      return dict;
    }
  }
}
