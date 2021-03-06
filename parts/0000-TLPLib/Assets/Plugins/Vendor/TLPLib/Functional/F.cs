﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional {
  public static partial class F {
    public static bool isNull<A>(A value) where A : class =>
      // This might seem to be overkill, but on the case of Transforms that
      // have been destroyed, target == null will return false, whereas
      // target.Equals(null) will return true.  Otherwise we don't really
      // get the benefits of the nanny.
      value == null || value.Equals(null);

    public static Option<A> opt<A>(A value) where A : class =>
      isNull(value) ? Option<A>.None : new Option<A>(value);

    public static Option<A> opt<A>(A? value) where A : struct =>
      value == null ? Option<A>.None : some((A) value);

    [PublicAPI] public static Option<A> some<A>(A value) => new Option<A>(value);
    [PublicAPI] public static Option<A> none<A>() => Option<A>.None;
    [PublicAPI] public static None none_ => new None();

    public static Either<A, B> left<A, B>(A value) { return new Either<A, B>(value); }
    public static Either<A, B> right<A, B>(B value) { return new Either<A, B>(value); }

    public static These<A, B> thiz<A, B>(A value) { return new These<A, B>(value); }
    public static These<A, B> that<A, B>(B value) { return new These<A, B>(value); }
    public static These<A, B> these<A, B>(A a, B b) { return new These<A, B>(a, b); }

    // Exception thrower which "returns" a value for use in expressions.
    public static A throws<A>(Exception ex) { throw ex; }
    public static A matchErr<A>(string paramName, string value) =>
      throws<A>(new ArgumentOutOfRangeException($"Unknown {paramName} value: '{value}'"));

    public static Option<A> matchErrOpt<A>(string paramName, string value) {
      Log.d.error($"Unknown {paramName} value: '{value}'");
      return none<A>();
    }

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

    public static KeyValuePair<K, V> kv<K, V>(K key, V value) =>
      new KeyValuePair<K, V>(key, value);

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

    public static LazyVal<A> lazy<A>(Fn<A> func, Act<A> afterInitialization = null) =>
      new LazyValImpl<A>(func, afterInitialization);

    public static LazyVal<A> loggedLazy<A>(
      string name, Fn<A> func, ILog log = null, Log.Level level = Log.Level.DEBUG
    ) => lazy(() => {
      var _log = log ?? Log.d;
      if (_log.willLog(level)) _log.log(level, $"Initiliazing lazy value: {name}");
      return func();
    });

    /// <summary>Lift a value into lazy type.</summary>
    public static LazyVal<A> lazyLift<A>(A a) => new NotReallyLazyVal<A>(a);

    public static Fn<Unit> actToFn(Action action) =>
      () => { action(); return unit; };

    public static Action andThen(this Action first, Action second) =>
      () => { first(); second(); };

    public static Action andThenSys(this Action first, Action second) =>
      () => { first(); second(); };

    public static Fn<B> andThen<A, B>(this Fn<A> first, Fn<A, B> second) =>
      () => second(first());

    static class EmptyArray<T> {
      public static readonly T[] value = new T[0];
    }

    public static T[] emptyArray<T>() => EmptyArray<T>.value;

    class EmptyDisposable : IDisposable {
      public void Dispose() { }
    }

    [PublicAPI] public static readonly IDisposable emptyDisposable = new EmptyDisposable();

    /// <summary>Representation of ! as a function.</summary>
    [PublicAPI] public static readonly Fn<bool, bool> invert = a => !a;
    /// <summary>Representation of && as a function.</summary>
    [PublicAPI] public static readonly Fn<bool, bool, bool> and2 = (a, b) => a && b;
    [PublicAPI] public static readonly Fn<bool, bool, bool, bool> and3 = (a, b, c) => a && b && c;
    [PublicAPI] public static readonly Fn<bool, bool, bool, bool, bool> and4 = (a, b, c, d) => a && b && c && d;

    /// <summary>Representation of || as a function.</summary>
    [PublicAPI] public static readonly Fn<bool, bool, bool> or2 = (a, b) => a || b;

    /// <summary>Representation of + as a function.</summary>
    [PublicAPI] public static readonly Fn<float, float, float> add2F = (a, b) => a + b;

    [PublicAPI] public static readonly Fn<bool, bool> i = a => a;
  }
}
