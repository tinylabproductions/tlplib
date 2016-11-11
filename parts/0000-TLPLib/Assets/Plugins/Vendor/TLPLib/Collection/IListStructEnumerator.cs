using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Collection {
  public struct IListStructEnumerator<C, A> : IEnumerator<A> where C : IList<A> {
    public readonly C list;
    int position;

    public IListStructEnumerator(C list) : this() {
      this.list = list;
      position = -1;
    }
      
    public bool MoveNext() {
      if (position + 1 >= list.Count) return false;
      position++;
      return true;
    }

    public void Reset() => position = -1;
    public A Current => list[position];

    object IEnumerator.Current => Current;
    public void Dispose() {}
  }
}