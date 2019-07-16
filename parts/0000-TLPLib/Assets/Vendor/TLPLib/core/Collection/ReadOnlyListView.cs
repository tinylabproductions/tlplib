using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.Collection {
  public class ReadOnlyListView<A> : IReadOnlyList<A> {
    readonly IList<A> backing;
    public ReadOnlyListView(IList<A> backing) { this.backing = backing; }

    public IEnumerator<A> GetEnumerator() => backing.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => backing.Count;
    public A this[int index] => backing.a(index);
  }
}