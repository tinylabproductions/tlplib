using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface IObservableQueue<A, out C> {
    void addLast(A a);
    A first { get; }
    A last { get; }
    void removeFirst();
    int count { get; }
    C collection { get; }
  }

  public class ObservableReadOnlyLinkedListQueue<A> 
    : IObservableQueue<A, ReadOnlyLinkedList<A>>
  {
    readonly LinkedList<A> buffer;
    public ReadOnlyLinkedList<A> collection { get; }

    public ObservableReadOnlyLinkedListQueue() {
      buffer = new LinkedList<A>();
      collection = new ReadOnlyLinkedList<A>(buffer);
    }

    public void addLast(A a) => buffer.AddLast(a);
    public void removeFirst() => buffer.RemoveFirst();
    public int count => buffer.Count;
    public A first => buffer.First.Value;
    public A last => buffer.Last.Value;
  }

  public class ObservableLambdaQueue<A, C> : IObservableQueue<A, C> {
    readonly Act<A> _addLast;
    readonly Action _removeFirst;
    readonly Fn<int> _count;
    readonly Fn<C> _collection;
    readonly Fn<A> _first, _last;

    public ObservableLambdaQueue(Act<A> addLast, Action removeFirst, Fn<int> count, Fn<C> collection, Fn<A> first, Fn<A> last) {
      _addLast = addLast;
      _removeFirst = removeFirst;
      _count = count;
      _collection = collection;
      _first = first;
      _last = last;
    }

    public void addLast(A a) => _addLast(a);
    public void removeFirst() => _removeFirst();
    public int count => _count();
    public C collection => _collection();
    public A first => _first();
    public A last => _last();
  }
}