using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class RoDict {
    public static RoDict<K, V> a<K, V>(IDictionary<K, V> dict) { return new RoDict<K, V>(dict);}
  }

  /* Read only dictionary */
  public struct RoDict<K, V> : IEquatable<RoDict<K, V>> {
    readonly IDictionary<K, V> underlying;

    #region Equality

    public bool Equals(RoDict<K, V> other) {
      return Equals(underlying, other.underlying);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is RoDict<K, V> && Equals((RoDict<K, V>) obj);
    }

    public override int GetHashCode() {
      return (underlying != null ? underlying.GetHashCode() : 0);
    }

    public static bool operator ==(RoDict<K, V> left, RoDict<K, V> right) { return left.Equals(right); }
    public static bool operator !=(RoDict<K, V> left, RoDict<K, V> right) { return !left.Equals(right); }

    sealed class UnderlyingEqualityComparer : IEqualityComparer<RoDict<K, V>> {
      public bool Equals(RoDict<K, V> x, RoDict<K, V> y) {
        return Equals(x.underlying, y.underlying);
      }

      public int GetHashCode(RoDict<K, V> obj) {
        return (obj.underlying != null ? obj.underlying.GetHashCode() : 0);
      }
    }

    static readonly IEqualityComparer<RoDict<K, V>> UnderlyingComparerInstance = new UnderlyingEqualityComparer();

    public static IEqualityComparer<RoDict<K, V>> underlyingComparer {
      get { return UnderlyingComparerInstance; }
    }

    #endregion

    public RoDict(IDictionary<K, V> underlying) { this.underlying = underlying; }

    public int count { get { return underlying.Count; } }
    public bool isEmpty { get { return count == 0; } }
    public bool nonEmpty { get { return ! isEmpty; } }
    public ICollection<K> keys { get { return underlying.Keys; } }
    public ICollection<V> values { get { return underlying.Values; } }
    public ICollection<KeyValuePair<K, V>> pairs { get { return underlying; } }
    public V this[K key] { get { return underlying.a(key); } }
    public Option<V> get(K key) { return underlying.get(key); }
    public bool contains(K key) { return underlying.ContainsKey(key); }

    public override string ToString() { return string.Format(
      "RoDict<{0}, {1}>(underlying: {2})", typeof(K), typeof(V), underlying.asString()
    ); }
  }
}
