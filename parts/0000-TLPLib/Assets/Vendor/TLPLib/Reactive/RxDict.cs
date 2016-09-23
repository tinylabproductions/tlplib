using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public struct DictValChange<V> {
    Option<V> oldValue;
    Option<V> newValue;

    DictValChange(Option<V> oldValue, Option<V> newValue) {
      this.oldValue = oldValue;
      this.newValue = newValue;
    }

    public Option<V> adding { get {
      return (oldValue.isEmpty && newValue.isDefined) ? newValue : F.none<V>();
    } }

    public Option<V> removal { get {
      return (oldValue.isDefined && newValue.isEmpty) ? oldValue : F.none<V>();
    } }

    public Option<Tpl<V, V>> updating { get {
      return (oldValue.isDefined && newValue.isDefined) 
        ? oldValue.zip(newValue) : F.none<Tpl<V, V>>();
    } }

    public static DictValChange<V> add(V newValue) {
      return new DictValChange<V>(F.none<V>(), F.some(newValue));
    }

    public static DictValChange<V> remove(V oldValue) {
      return new DictValChange<V>(F.some(oldValue), F.none<V>());
    }

    public static DictValChange<V> update(V oldValue, V newValue) {
      return new DictValChange<V>(F.some(oldValue), F.some(newValue));
    }
  }

  public interface IRxDictObs<K, V> {
    IObservable<KeyValuePair<K, V>> keyAdded { get; }
    IObservable<KeyValuePair<K, V>> keyRemoved { get; }
    IObservable<KeyValuePair<K, DictValChange<V>>> keyChanged { get; }
    IObservable<KeyValuePair<K, V>> keySet { get; }
  }

  public interface IRxDict<K, V> : IDictionary<K, V>, IRxDictObs<K, V> {}

  /* Reactive dictionary */
  public class RxDict<K, V> : IRxDict<K, V> {
    readonly Subject<KeyValuePair<K, V>> _keyAdded = 
      new Subject<KeyValuePair<K, V>>();
    /* Emits key and new value on key add. */
    public IObservable<KeyValuePair<K, V>> keyAdded { get { return _keyAdded; } }

    readonly Subject<KeyValuePair<K, V>> _keyRemoved = 
      new Subject<KeyValuePair<K, V>>();
    /* Emits key and current value on key removal. */
    public IObservable<KeyValuePair<K, V>> keyRemoved { get { return _keyRemoved; } }

    readonly Subject<KeyValuePair<K, DictValChange<V>>> _keyChanged =
      new Subject<KeyValuePair<K, DictValChange<V>>>();
    /* Emits key, previous and new value on key change (add/update/remove). */
    public IObservable<KeyValuePair<K, DictValChange<V>>> keyChanged 
      { get { return _keyChanged; } }

    readonly Subject<KeyValuePair<K, V>> _keySet = 
      new Subject<KeyValuePair<K, V>>();
    /* Emits key and new value on key set (change or add). */
    public IObservable<KeyValuePair<K, V>> keySet { get { return _keySet; } }

    readonly Dictionary<K, V> backing = new Dictionary<K, V>();
    ICollection<KeyValuePair<K, V>> backingC { get { return backing; } }

    public RxDict() {
      _keyAdded.subscribe(_keySet);
      _keyChanged.subscribe(kv => {
        foreach (var newValue in kv.Value.adding) _keyAdded.push(F.kv(kv.Key, newValue));
        foreach (var oldValue in kv.Value.removal) _keyRemoved.push(F.kv(kv.Key, oldValue));
        foreach (var t in kv.Value.updating) _keySet.push(F.kv(kv.Key, t._2));
      });
    }

    void removal(KeyValuePair<K, V> item) {
      _keyChanged.push(F.kv(item.Key, DictValChange<V>.remove(item.Value)));
    }

    void adding(K key, V value) {
      _keyChanged.push(F.kv(key, DictValChange<V>.add(value)));
    }

    void updating(K key, V oldValue, V newValue) {
      _keyChanged.push(F.kv(key, DictValChange<V>.update(oldValue, newValue)));
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() { return backing.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable) backing).GetEnumerator(); }

    public void Add(KeyValuePair<K, V> item) {
      backing.Add(item.Key, item.Value);
      _keyAdded.push(item);
    }

    public void Clear() {
      foreach (var kv in backing) removal(kv);
      backing.Clear();
    }

    public bool Contains(KeyValuePair<K, V> item) 
      { return backingC.Contains(item); }

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) 
      { backingC.CopyTo(array, arrayIndex); }

    public bool Remove(KeyValuePair<K, V> item) { 
      var success = backingC.Remove(item);
      if (success) removal(item);
      return success;
    }

    public int Count { get { return backing.Count; } }
    public bool IsReadOnly { get { return backingC.IsReadOnly; } }
    public bool ContainsKey(K key) { return backing.ContainsKey(key); }

    public void Add(K key, V value) {
      backing.Add(key, value);
      adding(key, value);
    }

    public bool Remove(K key) {
      return backing.get(key).fold(false, value => {
        backing.Remove(key);
        removal(F.kv(key, value));
        return true;
      });
    }

    public bool TryGetValue(K key, out V value) { return backing.TryGetValue(key, out value); }

    public V this[K key] {
      get { return backing.a(key); }
      set { backing.get(key).voidFold(
        () => Add(key, value),
        oldValue => {
          backing[key] = value;
          updating(key, oldValue, value);
        }
      ); }
    }

    public ICollection<K> Keys { get { return backing.Keys; } }
    public ICollection<V> Values { get { return backing.Values; } }
  }
}