using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Functional {
  public static partial class F {
    public static Option<A> opt<A>(A value) where A : class {
      // This might seem to be overkill, but on the case of Transforms that
      // have been destroyed, target == null will return false, whereas
      // target.Equals(null) will return true.  Otherwise we don't really
      // get the benefits of the nanny.
      return value == null || value.Equals(null)
        ? Option<A>.None : new Option<A>(value);
    }

    public static Option<A> some<A>(A value) => new Option<A>(value);
    public static Option<A> none<A>() => Option<A>.None;

    public static Either<A, B> left<A, B>(A value) { return new Either<A, B>(value); }
    public static Either<A, B> right<A, B>(B value) { return new Either<A, B>(value); }

    public static These<A, B> thiz<A, B>(A value) { return new These<A, B>(value); }
    public static These<A, B> that<A, B>(B value) { return new These<A, B>(value); }
    public static These<A, B> these<A, B>(A a, B b) { return new These<A, B>(a, b); }

    // Exception thrower which "returns" a value for use in expressions.
    public static A throws<A>(Exception ex) { throw ex; }
    // Function that can be used to throw exceptions.
    public static void doThrow(Exception ex) { throw ex; }

    public static Try<A> doTry<A>(Fn<A> f) {
      try { return scs(f()); }
      catch (Exception e) { return err<A>(e); }
    }
    public static Try<Unit> doTry(Action action) {
      return doTry(() => { action(); return unit; });
    }
    public static Try<A> scs<A>(A value) { return new Try<A>(value); }
    public static Try<A> err<A>(Exception ex) { return new Try<A>(ex); }

    public static KeyValuePair<K, V> kv<K, V>(K key, V value) {
      return new KeyValuePair<K, V>(key, value);
    }

    public static List<A> list<A>(params A[] args) {
      return new List<A>(args);
    }

    public static LinkedList<A> linkedList<A>(params A[] args) {
      return new LinkedList<A>(args);
    }

    public static ReadOnlyLinkedList<A> roLinkedList<A>(params A[] args) =>
      new ReadOnlyLinkedList<A>(new LinkedList<A>(args));

    public static HashSet<A> hashSet<A>(params A[] args) {
      var hs = new HashSet<A>();
      foreach (var a in args) hs.Add(a);
      return hs;
    }

    public static List<A> emptyList<A>(int capacity=0) {
      return new List<A>(capacity);
    }

    public static A[] arrayFill<A>(int size, Fn<int, A> creator) {
      var arr = new A[size];
      for (var idx = 0; idx < size; idx++) arr[idx] = creator(idx);
      return arr;
    }

    public static ImmutableArray<A> iArrayFill<A>(int size, Fn<int, A> creator) {
      var arr = ImmutableArray.CreateBuilder<A>(size);
      for (var idx = 0; idx < size; idx++) arr.Add(creator(idx));
      return arr.MoveToImmutable();
    }

    public static List<A> listFill<A>(int size, Fn<int, A> creator) {
      var list = new List<A>(size);
      for (var idx = 0; idx < size; idx++) list.Add(creator(idx));
      return list;
    }

    public static ImmutableList<A> iListFill<A>(int size, Fn<int, A> creator) {
      var lst = ImmutableList.CreateBuilder<A>();
      for (var idx = 0; idx < size; idx++) lst.Add(creator(idx));
      return lst.ToImmutable();
    }

    public static IList<A> ilist<A>(params A[] args) { return list(args); }

    public static Dictionary<K, V> dict<K, V>(params Tpl<K, V>[] args) {
      var dict = new Dictionary<K, V>();
      for (var idx = 0; idx < args.Length; idx++) {
        var tpl = args[idx];
        dict.Add(tpl._1, tpl._2);
      }
      return dict;
    }

    public static IDictionary<K, V> iDict<K, V>(params Tpl<K, V>[] args) => dict(args);

    public static Unit unit => Unit.instance;

    public static Lazy<A> lazy<A>(Fn<A> func) => new LazyImpl<A>(func);
    public static Lazy<A> lazy<A>(A a) => new NotReallyLazy<A>(a);

    public static Action andThen(this Action first, Action second) {
      return () => { first(); second(); };
    }

    public static Action andThenSys(this Action first, Action second) {
      return () => { first(); second(); };
    }

    public static Fn<B> andThen<A, B>(this Fn<A> first, Fn<A, B> second) {
      return () => second(first());
    }
  }
}
