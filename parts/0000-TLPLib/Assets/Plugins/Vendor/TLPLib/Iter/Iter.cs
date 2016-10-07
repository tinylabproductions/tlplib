#if ! ENABLE_IL2CPP
using System;
using System.Collections.Generic;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Iter {
  /**
   * Heap allocation free enumerator. Doesn't work on non-JIT platforms. 
   * What does work there?
   * 
   * Basic usage:
   * 
   * for (
   *   var i = list.iter(); // obtain an Iter
   *   i;                   // check if is valid
   *   i++                  // advance to next value
   * ) {
   *   var current = ~i; // Fetch current value.
   * }
   * 
   * Or alternatively you can use one of the methods defined on Iter.
   * 
   * Adding new operations:
   * 
   * Obviously check out existing implementations. One thing to be noted,
   * an Iter must always be returned initialized to first position. This is
   * contrary to Enumerator, which starts off uninitialized. In other words,
   * .current from newly returned Iter must always have a value if it is 
   * possible.
   * 
   * Credits:
   * 
   * Iter is written by Artūras Šlajus (arturaz, http://arturaz.net).
   * 
   * Iter is based on Smooth Foundations library 
   * (https://github.com/pdo400/smooth.foundations) written by 
   * Patrick Whitesell (pdo400).
   * 
   * It is an amazing piece of software, however I felt like the ability to 
   * remove items was making it too complicated to write new operations. So
   * this is my take on this problem.
   **/
  public struct Iter<A, Ctx> {
    #region Cache

    /* Function cache storage. */
    private static readonly IDictionary<string, FnCache> _fnCache = 
      new Dictionary<string, FnCache>();

    /* Several methods might use same Ctx signature, so we need to pass a diferentiator. */
    public static FnCache fnCache(string id) {
      FnCache cache;
      if (! _fnCache.TryGetValue(id, out cache)) {
        cache = new FnCache();
        _fnCache[id] = cache;
      }
      return cache;
    }

    /* Actual function cache that helps us cache fns and reduce boilerplate. */
    public class FnCache {
      public Fns fns;

      // ReSharper disable once MemberHidesStaticFromOuterClass
      public Iter<A, Ctx> empty { get { return Iter<A, Ctx>.empty; } }

      public Fns build(
        Fn<Ctx, Option<Ctx>> skipper, Fn<Ctx, A> getter, 
        Fn<Ctx, Option<int>> sizeHint
      ) { return new Fns(skipper, getter, sizeHint); }
    }

    /* Holder for Iter function cache. */
    public class Fns {
      public readonly Fn<Ctx, Option<Ctx>> skipper;
      public readonly Fn<Ctx, A> getter;
      public readonly Fn<Ctx, Option<int>> sizeHint;

      public Fns(Fn<Ctx, Option<Ctx>> skipper, Fn<Ctx, A> getter, Fn<Ctx, Option<int>> sizeHint) {
        this.skipper = skipper;
        this.getter = getter;
        this.sizeHint = sizeHint;
      }

      public Iter<A, Ctx> iter(Ctx ctx) { return new Iter<A, Ctx>(ctx, this); }
    }

    #endregion

    #region Internal state

    /* Takes a context and provides next context, which is None if Iter is 
     * done. */
    private readonly Fn<Ctx, Option<Ctx>> skipper;
    /* Extracts a value from context. */
    private readonly Fn<Ctx, A> getter;
    /* How many elements are left in this Iter? */
    private readonly Fn<Ctx, Option<int>> sizeHint;

    /* None if Iter is finished, Some if it's valid. */
    private Option<Ctx> state;

    #endregion

    #region Base interface

    public static readonly Iter<A, Ctx> empty = new Iter<A, Ctx>();

    public Iter(Ctx ctx, Fns fns) {
      state = F.some(ctx);
      skipper = fns.skipper;
      getter = fns.getter;
      sizeHint = fns.sizeHint;
    }

    public override string ToString() {
      return string.Format("Iter({0}|sh:{1})", state, elementsLeft);
    }

    /* Mutates Iter and moves it to next place in the sequence. */
    public void progress() { state = state.flatMap(skipper); }

    /* Does this Iter currently has a value? */
    public bool hasValue { get { return state.isDefined; } }

    /* Get the current value of Iter. Returns None if iter is finished. */
    public Option<A> current { get { return state.map(getter); } }

    /* Unsafe get the current value of Iter. 
     * Throws exception if iter is finished. */
    public A get { get {
      return current.getOrThrow(() => new IllegalStateException(string.Format(
        "Iter({0}, {1}) is finished.", typeof(A), typeof(Ctx))
      ));
    } }

    /* Skips Iter and returns new current element. */
    public Option<A> next { get {
      progress();
      return current;
    } }

    /* Returns how many elements are left in this Iter. 
     * Not always possible to know that (e.g. if iterating IEnumerable). */
    public Option<int> elementsLeft { get {
      return state.isEmpty ? F.some(0) : sizeHint(state.get);
    } }

    /* Alias to .hasValue */
    public static implicit operator bool(Iter<A, Ctx> iter) { return iter.hasValue; }

    /* Alias to .get */
    public static A operator ~(Iter<A, Ctx> iter) { return iter.get; }

    /* Alias to .skip() */
    public static Iter<A, Ctx> operator ++(Iter<A, Ctx> iter) {
      iter.progress();
      return iter;
    }

    #endregion
  }

  public static class IterOps {
    
    #region Conversions

    /* Iterates underlying sequence and returns a list from that. */
    public static List<A> toList<A, Ctx>(this Iter<A, Ctx> iter) {
      var list = iter.elementsLeft.fold(() => new List<A>(), F.emptyList<A>);
      for (var i = iter; i; i++) list.Add(~i);
      return list;
    }

    /* Same as toList() but returns an interface type. */
    public static IList<A> toIList<A, Ctx>(this Iter<A, Ctx> iter) { return iter.toList(); }

    /* Iterates underlying sequence and returns a linked list from that. */
    public static LinkedList<A> toLinkedList<A, Ctx>(this Iter<A, Ctx> iter) {
      var ll = new LinkedList<A>();
      for (var i = iter; i; i++) ll.AddLast(~i);
      return ll;
    }

    /* Iterates underlying sequence and returns an array from that.
     * 
     * Beware that this will cause in a temporary list for sources that do not
     * provide a size hint. */
    public static A[] toArray<A, Ctx>(this Iter<A, Ctx> iter) {
      var leftOpt = iter.elementsLeft;
      if (leftOpt.isDefined) {
        var arr = new A[leftOpt.get];
        var idx = 0;
        for (var i = iter; i; i++, idx++) arr[idx] = ~i;
        return arr;
      }
      else {
        var list = new List<A>();
        for (var i = iter; i; i++) list.Add(~i);
        return list.ToArray();
      }
    }

    #endregion

    #region Folds

    /* Folds this Iter to a value using given initial state and folder. */
    public static B foldLeft<A, Ctx, B>(this Iter<A, Ctx> iter, B initial, Fn<B, A, B> folder) {
      var current = initial;
      for (var i = iter; i; i++) current = folder(current, ~i);
      return current;
    }

    /* Reduces this Iter to a value using first element as initial 
     * state and given reducer.
     * 
     * Returns None if Iter is empty. */
    public static Option<A> reduceLeft<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, A, A> reducer) {
      return iter.reduceLeft(_ => _, reducer);
    }

    /* Reduces this Iter to a value using first element passed through 
     * initial state extractor as initial state and given reducer.
     * 
     * Returns None if Iter is empty. */
    public static Option<B> reduceLeft<A, Ctx, B>(this Iter<A, Ctx> iter, 
      Fn<A, B> initialStateExtractor, Fn<B, A, B> reducer
    ) {
      if (iter) {
        var i = iter;
        var initial = ~(i++);
        return F.some(i.foldLeft(initialStateExtractor(initial), reducer));
      }
      else return F.none<B>();
    }

    /* Reduces this Iter to a value using a predicate that tells us whether
     * we should keep left argument.
     * 
     * Returns None if Iter is empty. */
    public static Option<A> keepLeft<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, A, bool> keepLeft) {
      return iter.reduceLeft((a, b) => keepLeft(a, b) ? a : b);
    }

    /**
     * Returns tuple of linked lists where first one contains all the items
     * that matched the predicate and second - those who didn't.
     **/
    public static Tpl<LinkedList<A>, LinkedList<A>> partition<A, Ctx>(
      this Iter<A, Ctx> iter, Fn<A, bool> predicate
    ) {
      var trues = new LinkedList<A>();
      var falses = new LinkedList<A>();
      for (var i = iter; i; i++) {
        var a = ~i;
        if (predicate(a)) trues.AddLast(a);
        else falses.AddLast(a);
      }
      return F.t(trues, falses);
    }

    /* Iterates through sequence, extracts a float and returns average. 
     * Returns None if Iter is empty. */
    public static Option<float> avg<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, float> extractor) {
      if (! iter) return F.none<float>();

      var total = 0f;
      var count = 0;

      for (var i = iter; i; i++) {
        total += extractor(~i);
        count += 1;
      }

      return F.some(total / count);
    }

    /* Constructs a string from this Iter. */
    public static string mkString<A, Ctx>(
      this Iter<A, Ctx> iter, 
      string separator, string start = null, string end = null
    ) {
      var b = new StringBuilder();
      if (start != null) b.Append(start);
      var idx = 0;
      for (var i = iter; i; i++, idx++) {
        if (idx == 0) b.Append(~i);
        else {
          b.Append(separator);
          b.Append(~i);
        }
      }
      if (end != null) b.Append(end);

      return b.ToString();
    }

    #endregion

    #region Traversals

    /* Lambda style for each. */
    public static void each<A, Ctx>(this Iter<A, Ctx> iter, Act<A> f) 
    { for (var i = iter; i; i++) f(~i); }

    /* Lambda style for each with index. */
    public static void eachWithIndex<A, Ctx>(this Iter<A, Ctx> iter, Act<A, int> f) {
      var idx = 0;
      for (var i = iter; i; i++, idx++) f(~i, idx);
    }

    /* Filters Iter with given predicate. */
    public static Iter<A, Tpl<Iter<A, Ctx>, Fn<A, bool>>>
    filter<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, bool> __predicate) {
      // Find first item which satisfies given predicate.
      for (; iter && ! __predicate(~iter); iter++) {}

      var cache = Iter<A, Tpl<Iter<A, Ctx>, Fn<A, bool>>>.fnCache("filter");
      // If we're done with the iterator, return empty Iter.
      if (! iter) return cache.empty;

      // Otherwise return new iterator that filters elements.
      return (cache.fns ?? (cache.fns = cache.build(
        ctx => { var i = ctx._1; var p = ctx._2;
          // Skip to first next one where predicate is satisfied.
          while ((++i).hasValue && ! p(~i)) {}

          return i 
            ? F.some(F.t(i, p)) 
            : F.none<Tpl<Iter<A, Ctx>, Fn<A, bool>>>();
        },
        ctx => ~ctx._1,
        ctx => F.none<int>()
      ))).iter(F.t(iter, __predicate));
    }

    /* Maps type A to type B. */
    public static Iter<B, Tpl<Iter<A, Ctx>, Fn<A, B>>> map<A, Ctx, B>(
      this Iter<A, Ctx> iter, Fn<A, B> __mapper
    ) {
      var cache = Iter<B, Tpl<Iter<A, Ctx>, Fn<A, B>>>.fnCache("map");
      if (! iter) return cache.empty;

      return (cache.fns ?? (cache.fns = cache.build(
        ctx => { var i = ctx._1; var map = ctx._2;
          return (++i).hasValue
            ? F.some(F.t(i, map))
            : F.none<Tpl<Iter<A, Ctx>, Fn<A, B>>>();
        },
        ctx => ctx._2(~ctx._1),
        ctx => ctx._1.elementsLeft
      ))).iter(F.t(iter, __mapper));
    }

    /* Maps type A to types B Iter (multiple values) and flattens it to an 
     * Iter. */
    public static Iter<
      B, Tpl<Iter<A, Ctx>, Iter<B, BCtx>, Fn<A, Iter<B, BCtx>>>
    > flatMap<A, Ctx, B, BCtx>(this Iter<A, Ctx> _iter, Fn<A, Iter<B, BCtx>> __mapper) {
      var cache = Iter<
        B, Tpl<Iter<A, Ctx>, Iter<B, BCtx>, Fn<A, Iter<B, BCtx>>>
      >.fnCache("flatMap");

      // We might be empty.
      if (! _iter) return cache.empty;

      var _subIter = __mapper(~_iter);
      // Find first sub iterator which isn't empty.
      while (_iter && ! _subIter) {
        _iter++;
        if (_iter) _subIter = __mapper(~_iter);
      }

      // We might not have any of these, in which case our iter is invalid.
      if (! _iter) return cache.empty;

      return (cache.fns ?? (cache.fns = cache.build(
        ctx => { var i = ctx._1; var bi = ctx._2; var map = ctx._3;
          if (++bi) {
            // Subiterator has a value.
            var nCtx = F.some(F.t(i, bi, map));
            return nCtx;
          }
          else {
            // Find next point where a value exists.
            i++;
            bi = i ? map(~i) : Iter<B, BCtx>.empty;
            while (i && ! bi) {
              i++;
              if (i) bi = map(~i);
            }

            return bi ? F.some(F.t(i, bi, map)) : F.none<
              Tpl<Iter<A, Ctx>, Iter<B, BCtx>, Fn<A, Iter<B, BCtx>>>
            >();
          }
        },
        ctx => ~ctx._2,
        ctx => F.none<int>()
      ))).iter(F.t(_iter, _subIter, __mapper));
    }

    /* Skips N elements from this Iter. */
    public static Iter<A, Ctx> skip<A, Ctx>(this Iter<A, Ctx> iter, int count) {
      var i = iter;
      for (var idx = 0; idx < count; idx++) i++;
      return i;
    }

    /* Takes N elements from this Iter. */
    public static Iter<A, Tpl<Iter<A, Ctx>, int, int>> take<A, Ctx>(
      this Iter<A, Ctx> __iter, int __count
    ) {
      var cache = Iter<A, Tpl<Iter<A, Ctx>, int, int>>.fnCache("take");
      if (! __iter || __count < 1) return cache.empty;

      return (cache.fns ?? (cache.fns = cache.build(
        // ReSharper disable once RedundantTypeArgumentsOfMethod
        // Mono compiler bug.
        ctx => ctx.ua<Option<Tpl<Iter<A, Ctx>, int, int>>>((iter, taken, max) =>
          (taken < max && ++iter)
            ? F.some(F.t(iter, taken + 1, max))
            : F.none<Tpl<Iter<A, Ctx>, int, int>>()
        ),
        ctx => ~ctx._1,
        // ReSharper disable once RedundantTypeArgumentsOfMethod
        // Mono compiler bug.
        ctx => ctx.ua<Option<int>>((iter, taken, max) => {
          var leftOpt = iter.elementsLeft;
          var takeLeft = max - taken + 1;
          return F.some(
            leftOpt.isEmpty ? takeLeft : Math.Min(leftOpt.get, takeLeft)
          );
        }) 
      ))).iter(F.t(__iter, 1, __count) /* iter, taken, max */);
    }

    /* Iterates through Iter and returns first element that satisfies predicate. */
    public static Option<A> find<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, bool> predicate) {
      for (var i = iter; i; i++) {
        var current = ~i;
        if (predicate(current)) return F.some(current);
      }
      return F.none<A>();
    }

    /* Iterates through Iter and returns first element that satisfies predicate. */
    public static Option<A> find<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, int, bool> predicate) {
      var idx = 0;
      for (var i = iter; i; i++, idx++) {
        var current = ~i;
        if (predicate(current, idx)) return F.some(current);
      }
      return F.none<A>();
    }

    /* Iterates through Iter and returns first Some that finder returns. */
    public static Option<B> find<A, Ctx, B>(this Iter<A, Ctx> iter, Fn<A, Option<B>> finder) {
      for (var i = iter; i; i++) {
        var current = ~i;
        var found = finder(current);
        if (found.isDefined) return found;
      }
      return F.none<B>();
    }

    /* Iterates through Iter and returns first Some that finder returns. */
    public static Option<B> find<A, Ctx, B>(
      this Iter<A, Ctx> iter, Fn<A, int, Option<B>> finder
    ) {
      var idx = 0;
      for (var i = iter; i; i++, idx++) {
        var current = ~i;
        var found = finder(current, idx);
        if (found.isDefined) return found;
      }
      return F.none<B>();
    }

    /**
     * Iterates the collection. Tries to find a member using predicate. If it
     * doesn't find one, returns last element of enumerable.
     * 
     * Returns None if enumerable is empty.
     **/
    public static Option<A> findOrLast<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, bool> predicate) {
      var last = F.none<A>();
      for (var i = iter; i; i++) {
        var current = ~i;
        var currentOpt = F.some(current);
        if (predicate(current)) return currentOpt; 
        else last = currentOpt;
      }
      return last;
    }

    /* Returns first index where predicate matched. 
     * Returns None on empty Iter. */
    public static Option<int> indexWhere<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, bool> predicate) {
      var idx = 0;
      for (var i = iter; i; i++, idx++) if (predicate(~i)) return F.some(idx);
      return F.none<int>();
    }

    /* Returns a random element. The probability is selected by elements 
     * weight. */
    public static Option<A> randomElementByWeight<A, Ctx>(
      this Iter<A, Ctx> iter, 
      // If we change to Func here, Unity crashes. So fun.
      Fn<A, float> weightSelector
    ) {
      var totalWeightOpt = iter.reduceLeft(weightSelector, (s, i) => s + weightSelector(i));
      if (totalWeightOpt.isEmpty) return F.none<A>();

      var totalWeight = totalWeightOpt.get;
      // The weight we are after...
      var itemWeightIndex = Random.value * totalWeight;
      var currentWeightIndex = 0f;

      for (var i = iter; i; i++) {
        var a = ~i;
        currentWeightIndex += weightSelector(a);
        // If we've hit or passed the weight we are after for this item then it's the one we want....
        if (currentWeightIndex >= itemWeightIndex) return F.some(a);
      }

      throw new IllegalStateException();
    }

    #endregion

    #region Checks

    /* Checks if any element in this Iter satisfies given predicate. 
     * Returns false on empty Iter. */
    public static bool exists<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, bool> predicate) {
      for (var i = iter; i; i++) if (predicate(~i)) return true;
      return false;
    }

    /* Checks if given element is in this Iter. 
     * Returns false on empty Iter. */
    public static bool contains<A, Ctx>(this Iter<A, Ctx> iter, A element) {
      for (var i = iter; i; i++) 
        if (EqComparer<A>.Default.Equals(~i, element)) return true;
      return false;
    }

    /* Checks if all elements in this Iter satisfies given predicate. 
     * Returns true on empty Iter. */
    public static bool forall<A, Ctx>(this Iter<A, Ctx> iter, Fn<A, bool> predicate) {
      for (var i = iter; i; i++) if (! predicate(~i)) return false;
      return true;
    }

    #endregion

    #region Joins

    /* Zips two Iters. Emits values while this or other is exhausted. */
    public static Iter<Tpl<A, B>, Tpl<Iter<A, Ctx>, Iter<B, BCtx>>>
    zip<A, Ctx, B, BCtx>(this Iter<A, Ctx> __iter, Iter<B, BCtx> __other) {
      var cache = Iter<Tpl<A, B>, Tpl<Iter<A, Ctx>, Iter<B, BCtx>>>.fnCache("zip");
      if (! __iter || ! __other) return cache.empty;

      return (cache.fns ?? (cache.fns = cache.build(
        ctx => { var i1 = ctx._1; var i2 = ctx._2;
          return (++i1).hasValue && (++i2).hasValue
            ? F.some(F.t(i1, i2))
            : F.none<Tpl<Iter<A, Ctx>, Iter<B, BCtx>>>();
        },
        ctx => F.t(~ctx._1, ~ctx._2),
        ctx => 
          ctx._1.elementsLeft.zip(ctx._2.elementsLeft).
          map(t => Math.Min(t._1, t._2))
      ))).iter(F.t(__iter, __other));
    }

    /* Returns new Iter that emits (value, index) tuples. */
    public static Iter<Tpl<A, int>, Tpl<Iter<A, Ctx>, int>> zipWithIndex<A, Ctx>(
      this Iter<A, Ctx> __iter
    ) {
      var cache = Iter<Tpl<A, int>, Tpl<Iter<A, Ctx>, int>>.fnCache("zipWithIndex");
      if (! __iter) return cache.empty;
      return (cache.fns ?? (cache.fns = cache.build(
        ctx => { var iter = ctx._1; var idx = ctx._2;
          return (++iter).hasValue 
            ? F.some(F.t(iter, idx + 1)) 
            : F.none<Tpl<Iter<A, Ctx>, int>>();
        },
        ctx => F.t(~ctx._1, ctx._2),
        ctx => ctx._1.elementsLeft
      ))).iter(F.t(__iter, 0));
    }

    #endregion
    
    #region Context Adding

/**
 * Scala code to generate this part.
 * 
 
import java.io._

val iterCtx = new PrintWriter("IterCtx.cs")

(1 to 21).foreach { i =>
  val params = (1 to i).map(s => s"P$s").mkString(", ")
  val args = (1 to i).map(s => s"P$s _p$s").mkString(", ")
  val argVals = (1 to i).map(s => s"_p$s").mkString(", ")
  val ctxArgs = (1 to i).map(s => s"ctx._${s + 1}").mkString(", ")
  val emit = s"Tpl<A, $params>"
  
  iterCtx.println(s"""
  // Wrap Iter into a context with $i parameters.
  // Useful for avoiding heap alocations.
  public static Iter<$emit, Tpl<Iter<A, Ctx>, $params>> ctx<A, Ctx, $params>(this Iter<A, Ctx> iter, $args) {
    var cache = Iter<$emit, Tpl<Iter<A, Ctx>, $params>>.fnCache("ctx$i");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, $ctxArgs)) : F.none<Tpl<Iter<A, Ctx>, $params>>();
      },
      ctx => F.t(~ctx._1, $ctxArgs),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, $argVals));
  }
  """)
}
  
iterCtx.close()
 
 * 
 **/

  // Wrap Iter into a context with 1 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1>, Tpl<Iter<A, Ctx>, P1>> ctx<A, Ctx, P1>(this Iter<A, Ctx> iter, P1 _p1) {
    var cache = Iter<Tpl<A, P1>, Tpl<Iter<A, Ctx>, P1>>.fnCache("ctx1");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2)) : F.none<Tpl<Iter<A, Ctx>, P1>>();
      },
      ctx => F.t(~ctx._1, ctx._2),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1));
  }
  

  // Wrap Iter into a context with 2 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2>, Tpl<Iter<A, Ctx>, P1, P2>> ctx<A, Ctx, P1, P2>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2) {
    var cache = Iter<Tpl<A, P1, P2>, Tpl<Iter<A, Ctx>, P1, P2>>.fnCache("ctx2");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3)) : F.none<Tpl<Iter<A, Ctx>, P1, P2>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2));
  }
  

  // Wrap Iter into a context with 3 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3>, Tpl<Iter<A, Ctx>, P1, P2, P3>> ctx<A, Ctx, P1, P2, P3>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3) {
    var cache = Iter<Tpl<A, P1, P2, P3>, Tpl<Iter<A, Ctx>, P1, P2, P3>>.fnCache("ctx3");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3));
  }
  

  // Wrap Iter into a context with 4 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4>> ctx<A, Ctx, P1, P2, P3, P4>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4>>.fnCache("ctx4");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4));
  }
  

  // Wrap Iter into a context with 5 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5>> ctx<A, Ctx, P1, P2, P3, P4, P5>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5>>.fnCache("ctx5");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5));
  }
  

  // Wrap Iter into a context with 6 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6>>.fnCache("ctx6");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6));
  }
  

  // Wrap Iter into a context with 7 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7>>.fnCache("ctx7");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7));
  }
  

  // Wrap Iter into a context with 8 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8>>.fnCache("ctx8");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8));
  }
  

  // Wrap Iter into a context with 9 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9>>.fnCache("ctx9");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9));
  }
  

  // Wrap Iter into a context with 10 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>>.fnCache("ctx10");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10));
  }
  

  // Wrap Iter into a context with 11 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>>.fnCache("ctx11");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11));
  }
  

  // Wrap Iter into a context with 12 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>>.fnCache("ctx12");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12));
  }
  

  // Wrap Iter into a context with 13 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>>.fnCache("ctx13");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13));
  }
  

  // Wrap Iter into a context with 14 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>>.fnCache("ctx14");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14));
  }
  

  // Wrap Iter into a context with 15 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14, P15 _p15) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>>.fnCache("ctx15");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14, _p15));
  }
  

  // Wrap Iter into a context with 16 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14, P15 _p15, P16 _p16) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>>.fnCache("ctx16");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14, _p15, _p16));
  }
  

  // Wrap Iter into a context with 17 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14, P15 _p15, P16 _p16, P17 _p17) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17>>.fnCache("ctx17");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14, _p15, _p16, _p17));
  }
  

  // Wrap Iter into a context with 18 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14, P15 _p15, P16 _p16, P17 _p17, P18 _p18) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18>>.fnCache("ctx18");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14, _p15, _p16, _p17, _p18));
  }
  

  // Wrap Iter into a context with 19 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14, P15 _p15, P16 _p16, P17 _p17, P18 _p18, P19 _p19) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19>>.fnCache("ctx19");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19, ctx._20)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19, ctx._20),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14, _p15, _p16, _p17, _p18, _p19));
  }
  

  // Wrap Iter into a context with 20 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14, P15 _p15, P16 _p16, P17 _p17, P18 _p18, P19 _p19, P20 _p20) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20>>.fnCache("ctx20");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19, ctx._20, ctx._21)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19, ctx._20, ctx._21),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14, _p15, _p16, _p17, _p18, _p19, _p20));
  }
  

  // Wrap Iter into a context with 21 parameters.
  // Useful for avoiding heap alocations.
  public static Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20, P21>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20, P21>> ctx<A, Ctx, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20, P21>(this Iter<A, Ctx> iter, P1 _p1, P2 _p2, P3 _p3, P4 _p4, P5 _p5, P6 _p6, P7 _p7, P8 _p8, P9 _p9, P10 _p10, P11 _p11, P12 _p12, P13 _p13, P14 _p14, P15 _p15, P16 _p16, P17 _p17, P18 _p18, P19 _p19, P20 _p20, P21 _p21) {
    var cache = Iter<Tpl<A, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20, P21>, Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20, P21>>.fnCache("ctx21");

    if (! iter) return cache.empty;

    return (cache.fns ?? (cache.fns = cache.build(
      ctx => { var i = ctx._1;
        return ++i ? F.some(F.t(i, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19, ctx._20, ctx._21, ctx._22)) : F.none<Tpl<Iter<A, Ctx>, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16, P17, P18, P19, P20, P21>>();
      },
      ctx => F.t(~ctx._1, ctx._2, ctx._3, ctx._4, ctx._5, ctx._6, ctx._7, ctx._8, ctx._9, ctx._10, ctx._11, ctx._12, ctx._13, ctx._14, ctx._15, ctx._16, ctx._17, ctx._18, ctx._19, ctx._20, ctx._21, ctx._22),
      ctx => ctx._1.elementsLeft
    ))).iter(F.t(iter, _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9, _p10, _p11, _p12, _p13, _p14, _p15, _p16, _p17, _p18, _p19, _p20, _p21));
  }
  

  
    #endregion

  }
}
#endif