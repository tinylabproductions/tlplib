using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class ReadOnlyLinkedList {
    public static ReadOnlyLinkedList<A> a<A>(LinkedList<A> backingList) {
      return new ReadOnlyLinkedList<A>(backingList);
    }
  }

  [ComVisible(false)]
  [DebuggerDisplay("Count = {Count}")]
  public
#if ENABLE_IL2CPP
    class
#else
    struct
#endif
  ReadOnlyLinkedList<A> : IEnumerable<A>, IEquatable<ReadOnlyLinkedList<A>> {
    readonly LinkedList<A> backing;

    public ReadOnlyLinkedList(LinkedList<A> backingList) {
      backing = backingList;
    }

    #region Equality

    public bool Equals(ReadOnlyLinkedList<A> other) {
      return Equals(backing, other.backing);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ReadOnlyLinkedList<A> && Equals((ReadOnlyLinkedList<A>) obj);
    }

    public override int GetHashCode() {
      return (backing != null ? backing.GetHashCode() : 0);
    }

    public static bool operator ==(ReadOnlyLinkedList<A> left, ReadOnlyLinkedList<A> right) { return left.Equals(right); }
    public static bool operator !=(ReadOnlyLinkedList<A> left, ReadOnlyLinkedList<A> right) { return !left.Equals(right); }

    #endregion

    public IEnumerator<A> GetEnumerator() { return backing.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    public bool Contains(A item) { return backing.Contains(item); }

    public void CopyTo(A[] array, int arrayIndex) {
      backing.CopyTo(array, arrayIndex);
    }

    public int Count { get { return backing.Count; } }

    public LinkedListNode<A> First { get { return backing.First; } }
    public LinkedListNode<A> Last { get { return backing.Last; } }
    public LinkedListNode<A> Find(A value) { return backing.Find(value); }
    public LinkedListNode<A> FindLast(A value) 
      { return backing.FindLast(value); }
  }
}
