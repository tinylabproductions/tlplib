using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class RoDict {
    public static RoDict<K, V> a<K, V>(IDictionary<K, V> dict) { return new RoDict<K, V>(dict);}
  }

  /* Read only dictionary */
  public struct RoDict<K, V> {
    readonly IDictionary<K, V> underlying;

    public RoDict(IDictionary<K, V> underlying) { this.underlying = underlying; }

    public int count { get { return underlying.Count; } }
    public bool isEmpty { get { return count == 0; } }
    public bool nonEmpty { get { return ! isEmpty; } }
    public IEnumerable<K> keys { get { return underlying.Keys; } }
    public IEnumerable<V> values { get { return underlying.Values; } }
    public Option<V> get(K key) { return underlying.get(key); }

    public override string ToString() { return string.Format(
      "RoDict<{0}, {1}>(underlying: {2})", typeof(K), typeof(V), underlying.asString()
    ); }
  }
}
