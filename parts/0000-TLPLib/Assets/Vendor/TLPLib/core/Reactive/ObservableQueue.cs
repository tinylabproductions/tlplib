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
    readonly Action<A> _addLast;
    readonly Action _removeFirst;
    readonly Func<int> _count;
    readonly Func<C> _collection;
    readonly Func<A> _first, _last;

    public ObservableLambdaQueue(Action<A> addLast, Action removeFirst, Func<int> count, Func<C> collection, Func<A> first, Func<A> last) {
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