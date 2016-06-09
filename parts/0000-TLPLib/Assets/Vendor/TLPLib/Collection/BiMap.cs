using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Collection {
  public class BiMap<A, B> : IEnumerable<KeyValuePair<A, B>>  {
    readonly Dictionary<A, B> a2b = new Dictionary<A, B>();
    readonly Dictionary<B, A> b2a = new Dictionary<B, A>();

    public bool Add(A a, B b) {
      var containsA = a2b.ContainsKey(a);
      var containsB = b2a.ContainsKey(b);

      if (! containsA && ! containsB) {
        a2b[a] = b;
        b2a[b] = a;
        return true;
      }
      else if (containsA && ! containsB) {
        throw new ArgumentException($"Trying to replace {a} -> {a2b[a]} with {a} -> {b}!");
      }
      else if (! containsA && containsB) {
        throw new ArgumentException($"Trying to replace {b2a[b]} -> {b} with {a} -> {b}!");
      }
      else 
        return false;
    }
    public bool Add(B b, A a) { return Add(a, b); }

    public Option<B> get(A key) { return a2b.get(key); }
    public Option<A> get(B key) { return b2a.get(key); }

    public B this[A key] {
      get { return a2b.a(key); }
      set { Add(key, value); }
    }

    public A this[B key] {
      get { return b2a.a(key); }
      set { Add(value, key); }
    }

    public Dictionary<A, B>.KeyCollection AKeys => a2b.Keys;
    public Dictionary<B, A>.KeyCollection BKeys => b2a.Keys;

    IEnumerator<KeyValuePair<A, B>> IEnumerable<KeyValuePair<A, B>>.GetEnumerator() {
      return a2b.GetEnumerator();
    }

    public IEnumerator GetEnumerator() {
      return a2b.GetEnumerator();
    }
  }
}
