using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class ReadOnlyDictionary {
    public static ReadOnlyDictionary<K, V> a<K, V>(IDictionary<K, V> backing) =>
      new ReadOnlyDictionary<K, V>(backing);

    public static ReadOnlyDictionary<K, V> asReadOnly<K, V>(this IDictionary<K, V> backing) =>
      a(backing);
  }

  public class ReadOnlyDictionary<K, V> : IReadOnlyDictionary<K, V> {
    public static ReadOnlyDictionary<K, V> empty = new ReadOnlyDictionary<K, V>(new Dictionary<K, V>());

    public readonly IDictionary<K, V> __unsafeBackingDictionary;
    public ReadOnlyDictionary(IDictionary<K, V> backing) { __unsafeBackingDictionary = backing; }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => __unsafeBackingDictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => __unsafeBackingDictionary.Count;
    public bool ContainsKey(K key) => __unsafeBackingDictionary.ContainsKey(key);
    public bool TryGetValue(K key, out V value) => __unsafeBackingDictionary.TryGetValue(key, out value);
    public V this[K key] => __unsafeBackingDictionary.a(key);
    public IEnumerable<K> Keys => __unsafeBackingDictionary.Keys;
    public IEnumerable<V> Values => __unsafeBackingDictionary.Values;
  }
}