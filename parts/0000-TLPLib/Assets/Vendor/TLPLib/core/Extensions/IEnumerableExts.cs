using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.data;
using Smooth.Collections;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class IEnumerableExts {

    
    public static IEnumerable<A> Concat<A>(this IEnumerable<A> e, A a) => e.Concat(a.Yield());
    
    public static IEnumerable<A> Concat<A>(this IEnumerable<A> e, Option<A> aOpt) =>
      aOpt.isSome ? e.Concat(aOpt.__unsafeGet) : e;

    
    public static IEnumerable<Base> Concat2<Child, Base>(
      this IEnumerable<Child> e1, IEnumerable<Base> e2
    ) where Child : Base {
      foreach (var _child in e1) yield return _child;
      foreach (var _base in e2) yield return _base;
    }

    
    public static IEnumerable<Base> Concat3<Base, Child>(
      this IEnumerable<Base> e1, IEnumerable<Child> e2
    ) where Child : Base {
      foreach (var _base in e1) yield return _base;
      foreach (var _child in e2) yield return _child;
    }

    
    public static IEnumerable<A> Yield<A>(this A any) { yield return any; }

    public static Option<A> find<A>(this IEnumerable<A> enumerable, Func<A, bool> predicate) {
      foreach (var a in enumerable) if (predicate(a)) return F.some(a);
      return F.none<A>();
    }

    public static bool findOut<A>(this IEnumerable<A> enumerable, Func<A, bool> predicate, out A a) {
      foreach (var _a in enumerable)
        if (predicate(_a)) {
          a = _a;
          return true;
        }

      a = default;
      return false;
    }

    public static bool findOut<A, Data>(
      this IEnumerable<A> enumerable, Data data, Func<A, Data, bool> predicate, out A a
    ) {
      foreach (var _a in enumerable)
        if (predicate(_a, data)) {
          a = _a;
          return true;
        }

      a = default;
      return false;
    }
    
    public static Option<A> find<A, B>(
      this IEnumerable<A> enumerable, Func<A, B> mapper, B toFind, IEqualityComparer<B> comparer = null
    ) {
      comparer = comparer ?? EqualityComparer<B>.Default;
      foreach (var a in enumerable) {
        var b = mapper(a);
        if (comparer.Equals(b, toFind)) return F.some(a);
      }
      return F.none<A>();
    }

    
    public static IEnumerable<C> zip<A, B, C>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable, Func<A, B, C> zipper
    ) {
      var aEnum = aEnumerable.GetEnumerator();
      var bEnum = bEnumerable.GetEnumerator();

      while (aEnum.MoveNext() && bEnum.MoveNext())
        yield return zipper(aEnum.Current, bEnum.Current);

      aEnum.Dispose();
      bEnum.Dispose();
    }

    
    public static IEnumerable<C> zipLeft<A, B, C>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable, Func<A, B, C> zipper, Func<A, int, C> generateMissing
    ) {
      var aEnum = aEnumerable.GetEnumerator();
      var bEnum = bEnumerable.GetEnumerator();

      var idx = -1;
      while (aEnum.MoveNext()) {
        idx++;
        yield return bEnum.MoveNext() ? zipper(aEnum.Current, bEnum.Current) : generateMissing(aEnum.Current, idx);
      }

      aEnum.Dispose();
      bEnum.Dispose();
    }

    
    public static IEnumerable<C> zipRight<A, B, C>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable, Func<A, B, C> zipper, Func<B, int, C> generateMissing
    ) => bEnumerable.zipLeft(aEnumerable, (b, a) => zipper(a, b), generateMissing);

    
    public static IEnumerable<Tpl<A, B>> zip<A, B>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable
    ) => aEnumerable.zip(bEnumerable, F.t);

    
    public static IEnumerable<Tpl<A, int>> zipWithIndex<A>(this IEnumerable<A> enumerable) {
      var idx = 0;
      foreach (var a in enumerable) {
        yield return F.t(a, idx);
        idx += 1;
      }
    }
    
    
    public static IEnumerable<A> flatten<A>(this IEnumerable<Option<A>> enumerable) =>
      from aOpt in enumerable
      where aOpt.isSome
      select aOpt.__unsafeGet;

    
    public static IEnumerable<A> flatten<A>(this IEnumerable<IEnumerable<A>> enumerable) =>
      enumerable.SelectMany(_ => _);

    /// <summary>
    /// Maps enumerable invoking mapper once per distinct A.
    /// </summary>
    
    public static IEnumerable<B> mapDistinct<A, B>(
      this IEnumerable<A> enumerable, Func<A, B> mapper
    ) {
      var cache = new Dictionary<A, B>();
      foreach (var a in enumerable) {
        B b;
        if (!cache.TryGetValue(a, out b)) {
          b = mapper(a);
          cache.Add(a, b);
        }

        yield return b;
      }
    }

    public static HashSet<A> toHashSet<A>(this IEnumerable<A> enumerable) =>
      new HashSet<A>(enumerable);

    /// <summary>Partitions enumerable into two lists using a predicate.</summary>
    
    public static Partitioned<A> partition<A>(this IEnumerable<A> enumerable, Func<A, bool> predicate) {
      var trues = ImmutableList.CreateBuilder<A>();
      var falses = ImmutableList.CreateBuilder<A>();
      foreach (var a in enumerable) (predicate(a) ? trues : falses).Add(a);
      return Partitioned.a(trues.ToImmutable(), falses.ToImmutable());
    }

    
    public static Tpl<ImmutableList<A>, ImmutableList<B>> partitionCollect<A, B>(
      this IEnumerable<A> enumerable, Func<A, Option<B>> collector
    ) {
      var nones = ImmutableList.CreateBuilder<A>();
      var somes = ImmutableList.CreateBuilder<B>();
      foreach (var a in enumerable) {
        var bOpt = collector(a);
        if (bOpt.isSome)
          somes.Add(bOpt.__unsafeGet);
        else
          nones.Add(a);
      }
      return F.t(nones.ToImmutable(), somes.ToImmutable());
    }

    public static IOrderedEnumerable<A> OrderBySafe<A>(this IEnumerable<A> source, Comparable<A> cmp) => 
      source.OrderBy(a => a, cmp);

    /// <summary>Take <see cref="count"/> random unique elements from a finite enumerable.</summary>
    public static ImmutableList<A> takeRandomly<A>(
      this IEnumerable<A> enumerable, int count, ref Rng rng
    ) {
      var r = rng;
      // Need to force the evaluation to update rng state.
      var result = enumerable.OrderBySafe(_ => r.nextInt(out r)).Take(count).ToImmutableList();
      rng = r;
      return result;
    }

    public static bool isEmptyAllocating<A>(this IEnumerable<A> enumerable) => !enumerable.Any();
    public static bool nonEmptyAllocating<A>(this IEnumerable<A> enumerable) => enumerable.Any();
    
    public static IEnumerable<A> Except<A>(
      this IEnumerable<A> enumerable, A except, IEqualityComparer<A> cmp = null
    ) {
      cmp = cmp ?? EqualityComparer<A>.Default;
      return enumerable.Where(a => !cmp.Equals(a, except));
    }

    public static IEnumerable<A> Except<A>(
      this IEnumerable<A> collection, Option<A> maybeExcept, IEqualityComparer<A> cmp = null
    ) => maybeExcept.valueOut(out var a) ? collection.Except(a, cmp) : collection;
    
    public static Option<A> headOption<A>(this IEnumerable<A> enumerable) {
      foreach (var a in enumerable)
        return F.some(a);
      return F.none<A>();
    }
    /// <summary>
    /// Aggregate with index passed into reducer.
    /// </summary>
    
    public static B Aggregate<A, B>(
      this IEnumerable<A> enumerable, B initial, Func<A, B, int, B> reducer
    ) {
      var reduced = initial;
      var idx = 0;
      foreach (var a in enumerable) {
        reduced = reducer(a, reduced, idx);
        idx++;
      }

      return reduced;
    }

    /// <summary>
    /// Given an enumerable yield elements in groups of <see cref="windowSize"/>. 
    /// </summary>
    /// <example>
    /// new []{1,2,3,4}.slidingWindow(3)
    ///
    /// Will result in:
    ///
    /// new []{ new[]{1,2,3}, new []{2,3,4} }
    /// </example>
    
    public static IEnumerable<A[]> slidingWindow<A>(
      this IEnumerable<A> enumerable, uint windowSize
    ) {
      if (windowSize == 0) 
        throw new ArgumentException("Window size can't be 0!", nameof(windowSize));
      
      var window = new A[windowSize];
      var idx = 0;
      foreach (var a in enumerable) {
        if (windowSize > 1) {
          window.shiftLeft(1);
        }
        window[windowSize - 1] = a;

        if (idx >= windowSize - 1) {
          yield return window.clone();
        }
        idx++;
      }
    } 
  }

  public struct Partitioned<A> : IEquatable<Partitioned<A>> {
    public readonly ImmutableList<A> trues, falses;

    public Partitioned(ImmutableList<A> trues, ImmutableList<A> falses) {
      this.trues = trues;
      this.falses = falses;
    }

    public void Deconstruct(out ImmutableList<A> trues, out ImmutableList<A> falses) {
      trues = this.trues;
      falses = this.falses;
    }

    #region Equality

    public bool Equals(Partitioned<A> other) {
      return trues.SequenceEqual(other.trues) && falses.SequenceEqual(other.falses);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Partitioned<A> && Equals((Partitioned<A>) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return ((trues != null ? trues.GetHashCode() : 0) * 397) ^ (falses != null ? falses.GetHashCode() : 0);
      }
    }

    public static bool operator ==(Partitioned<A> left, Partitioned<A> right) { return left.Equals(right); }
    public static bool operator !=(Partitioned<A> left, Partitioned<A> right) { return !left.Equals(right); }

    #endregion

    public override string ToString() =>
      $"{nameof(Partitioned)}[" +
      $"{nameof(trues)}: {trues.asDebugString()}, " +
      $"{nameof(falses)}: {falses.asDebugString()}" +
      $"]";
  }

  public static class Partitioned {
    public static Partitioned<A> a<A>(ImmutableList<A> trues, ImmutableList<A> falses) =>
      new Partitioned<A>(trues, falses);
  }
}