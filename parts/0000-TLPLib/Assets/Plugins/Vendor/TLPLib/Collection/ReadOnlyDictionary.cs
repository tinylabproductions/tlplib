using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class ReadOnlyDictionary {
    public static IReadOnlyDictionary<K, V> a<K, V>(IDictionary<K, V> backing) =>
      new ReadOnlyDictionary<K, V>(backing);

    public static IReadOnlyDictionary<K, V> asReadOnly<K, V>(this IDictionary<K, V> backing) =>
      a(backing);
  }

  public class ReadOnlyDictionary<K, V> : IReadOnlyDictionary<K, V> {
    readonly IDictionary<K, V> backing;
    public ReadOnlyDictionary(IDictionary<K, V> backing) { this.backing = backing; }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => backing.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => backing.Count;
    public bool ContainsKey(K key) => backing.ContainsKey(key);
    public bool TryGetValue(K key, out V value) => backing.TryGetValue(key, out value);
    public V this[K key] => backing.a(key);
    public IEnumerable<K> Keys => backing.Keys;
    public IEnumerable<V> Values => backing.Values;
  }
}