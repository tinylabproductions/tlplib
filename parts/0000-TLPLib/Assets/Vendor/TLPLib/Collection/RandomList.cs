using System;
using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Collection {
  /* A list which has random order inside. 
   * 
   * Insertions: O(1), unless buffer expansion is needed.
   * Removals: O(1).
   * Traversals: O(n).
   */
  public class RandomList<A> : IList<A> {
    private readonly List<A> backing = new List<A>();

    public void Add(A item) { backing.Add(item); }
    public void Clear() { backing.Clear(); }
    public bool Contains(A item) { return backing.Contains(item); }
    public void CopyTo(A[] array, int arrayIndex) { backing.CopyTo(array, arrayIndex); }
    public int IndexOf(A item) { return backing.IndexOf(item); }
    public int Count { get { return backing.Count; } }
    public bool IsReadOnly { get { return false; } }
    public IEnumerator<A> GetEnumerator() { return backing.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    public A this[int index] {
      get { return backing[index]; }
      set { backing[index] = value; }
    }

    /* Insert element at index, moving current element to the end if it exists. */
    public void Insert(int index, A item) {
      // Delegate end and over the end insertions to backing.
      if (index >= backing.Count) backing.Insert(index, item);
      else {
        var current = backing[index];
        backing[index] = item;
        backing.Add(current);
      }
    }

    /* Removes first occurence of element, moving last element to its position. */
    public bool Remove(A item) {
      var index = backing.IndexOf(item);
      if (index == -1) return false;
      RemoveAt(index);
      return true;
    }

    /* Removes element at index, moving last element to its position. */
    public void RemoveAt(int index) {
      var lastIndex = backing.Count - 1;
      if (index == lastIndex) {
        backing.RemoveAt(lastIndex);
      }
      else {
        backing[index] = backing[lastIndex];
        backing.RemoveAt(lastIndex);
      }
    }

    /* Removes elements where predicate returns true. */
    public void RemoveWhere(Fn<A, bool> predicate) {
      var idx = 0;
      while (idx < Count) {
        var item = backing[idx];
        if (predicate(item)) RemoveAt(idx);
        else idx++;
      }
    }
  }
}
