using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * List where size and specific element indices can be observed over time.
   **/
  public interface IRxList<A> : IList<A> {
    IRxVal<int> rxSize { get; }
    IRxVal<Option<A>> rxElement(int index);
  }

  public class RxList<A> : IRxList<A> {
    private readonly List<A> backing = new List<A>();

    private readonly Dictionary<int, IRxRef<Option<A>>> elementRxRefs = 
      new Dictionary<int, IRxRef<Option<A>>>();

    private readonly IRxRef<int> _rxSize;
    public IRxVal<int> rxSize { get { return _rxSize; } }

    public RxList() {
      _rxSize = RxRef.a(0);
    }

    public IEnumerator<A> GetEnumerator() {
      return backing.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public virtual IRxVal<Option<A>> rxElement(int index) {
      return elementRxRefs.get(index).fold(
        () => {
          var rxRef = RxRef.a(this.get(index));
          // The cast is mono bug.
          // ArrayTypeMismatchException: Source array type cannot be assigned to destination array type.
          // (wrapper stelemref) object:stelemref (object,intptr,object)
          elementRxRefs[index] = rxRef;
          return rxRef;
        },
        _ => _
      );
    }

    public void Add(A item) {
      backing.Add(item);
      notifyElementObservable(Count);
      _rxSize.value = Count;
    }

    public void Clear() {
      backing.Clear();
      foreach (var kv in elementRxRefs) kv.Value.value = F.none<A>();
      _rxSize.value = Count;
    }

    public void Replace(IEnumerable<A> enumerable) {
      backing.Clear();
      backing.AddRange(enumerable);
      foreach (var kv in elementRxRefs) kv.Value.value = backing.get(kv.Key);
      _rxSize.value = Count;
    }

    public bool Contains(A item) {
      return backing.Contains(item);
    }

    public void CopyTo(A[] array, int arrayIndex) {
      backing.CopyTo(array, arrayIndex);
    }

    public bool Remove(A item) {
      var index = IndexOf(item);
      if (index < 0) return false;
      RemoveAt(index);
      _rxSize.value = Count;
      return true;
    }

    public int Count { get { return backing.Count; }}
    public bool IsReadOnly { get { return false; } }
    public int IndexOf(A item) {
      return backing.IndexOf(item);
    }

    public void Insert(int index, A item) {
      backing.Insert(index, item);
      for (var i = index; i < Count; i++) {
        notifyElementObservable(i);
      }
      _rxSize.value = Count;
    }

    public void RemoveAt(int index) {
      backing.RemoveAt(index);
      for (var i = index; i < Count + 1; i++) {
        notifyElementObservable(i);
      }
      _rxSize.value = Count;
    }

    public A this[int index] {
      get { return backing[index]; }
      set {
        backing[index] = value;
        notifyElementObservable(index);
      }
    }

    void notifyElementObservable(int index) {
      foreach (var t in elementRxRefs.get(index)) t.value = this.get(index);
    }
  }
}