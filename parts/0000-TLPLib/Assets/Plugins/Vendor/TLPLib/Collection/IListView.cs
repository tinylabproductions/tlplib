using System;
using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class IListView {
    public static IListView<A> a<A>(
      IList<A> list, int startIndex, int count
    ) => new IListView<A>(list, startIndex, count);
  }

  /**
   * Read-only view into IList.
   */
  // TODO: test
  public struct IListView<A> : IList<A>, IReadOnlyList<A> {
    readonly IList<A> list;
    readonly int startIndex;
    public int Count { get; }

    public IListView(IList<A> list, int startIndex, int count) {
      this.list = list;
      this.startIndex = startIndex;
      Count = count;
    }

    int toListIdx(int idx) {
      if (idx >= Count) throw new ArgumentOutOfRangeException(
        nameof(idx), $"{nameof(idx)}({idx}) >= {nameof(Count)}({Count})"
      );
      return startIndex + idx;
    }

    public IEnumerator<A> GetEnumerator() {
      for (var idx = 0; idx < Count; idx++)
        yield return this[idx];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region Unsupported

    public void Add(A item) { throw new NotSupportedException(); }
    public void Clear() { throw new NotSupportedException(); }
    public bool Remove(A item) { throw new NotSupportedException(); }
    public void Insert(int index, A item) { throw new NotSupportedException(); }
    public void RemoveAt(int index) { throw new NotSupportedException(); }

    #endregion

    public bool Contains(A item) => IListDefaultImpls.contains(this, item);

    public void CopyTo(A[] array, int arrayIndex) =>
      IListDefaultImpls.copyTo(this, array, arrayIndex, startIndex, Count);

    public bool IsReadOnly => true;

    public int IndexOf(A item) => IListDefaultImpls.indexOf(this, item);

    public A this[int index] {
      get { return list[toListIdx(index)]; }
      set { throw new NotSupportedException(); }
    }
  }
}