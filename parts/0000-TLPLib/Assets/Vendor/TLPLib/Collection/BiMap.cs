using System;
using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Collection {
  public class BiMap<A, B> : IEnumerable<KeyValuePair<A, B>>  {
    private readonly Dictionary<A, B> a2b = new Dictionary<A, B>();
    private readonly Dictionary<B, A> b2a = new Dictionary<B, A>();

    public bool Add(A a, B b) {
      var containsA = a2b.ContainsKey(a);
      var containsB = b2a.ContainsKey(b);

      if (! containsA && ! containsB) {
        a2b[a] = b;
        b2a[b] = a;
        return true;
      }
      else if (containsA && ! containsB) {
        throw new ArgumentException(String.Format(
          "Trying to replace {0} -> {1} with {0} -> {2}!",
          a, a2b[a], b
        ));
      }
      else if (! containsA && containsB) {
        throw new ArgumentException(String.Format(
          "Trying to replace {0} -> {1} with {2} -> {1}!",
          b2a[b], b, a
        ));
      }
      else 
        return false;
    }

    public bool Add(B b, A a) {
      return Add(a, b);
    }

    public B Get(A key) {
      return a2b[key];
    }

    public A Get(B key) {
      return b2a[key];
    }

    public B this[A key] {
      get { return Get(key); }
      set { Add(key, value); }
    }

    public A this[B key] {
      get { return Get(key); }
      set { Add(value, key); }
    }

    public Dictionary<A, B>.KeyCollection AKeys {
      get { return a2b.Keys; }
    }

    public Dictionary<B, A>.KeyCollection BKeys {
      get { return b2a.Keys; }
    }

    IEnumerator<KeyValuePair<A, B>> IEnumerable<KeyValuePair<A, B>>.GetEnumerator() {
      return a2b.GetEnumerator();
    }

    public IEnumerator GetEnumerator() {
      return a2b.GetEnumerator();
    }
  }
}
