using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Collection {
  public class HeapImmutableArray<A> : IEnumerable<A> {
    [PublicAPI] public readonly ImmutableArray<A> backing;

    public HeapImmutableArray(ImmutableArray<A> backing) { this.backing = backing; }

    public Enumerator GetEnumerator() => new Enumerator(backing);
    IEnumerator<A> IEnumerable<A>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<A> {
      readonly ImmutableArray<A> _array;
      int _index;

      internal Enumerator(ImmutableArray<A> array) {
        _array = array;
        _index = -1;
      }

      public A Current => _array[_index];
      object IEnumerator.Current => Current;
      public bool MoveNext() => ++_index < _array.Length;

      public void Reset() => _index = -1;
      public void Dispose() {}
    }
  }

  public static class HeapImmutableArrayExts {
    [PublicAPI]
    public static HeapImmutableArray<A> toHeap<A>(this ImmutableArray<A> arr) => 
      new HeapImmutableArray<A>(arr);
  }
}