using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class ImmutableArrayExts {
    public static ImmutableArray<To> map<From, To>(
      this ImmutableArray<From> source, Func<From, To> mapper
    ) {
      var target = ImmutableArray.CreateBuilder<To>(source.Length);
      for (var i = 0; i < source.Length; i++) target.Add(mapper(source[i]));
      return target.MoveToImmutable();
    }

    public static Option<T> get<T>(this ImmutableArray<T> list, int index) =>
      index >= 0 && index < list.Length ? F.some(list[index]) : F.none<T>();

    public static bool isEmpty<A>(this ImmutableArray<A> list) => list.Length == 0;
    public static bool nonEmpty<A>(this ImmutableArray<A> list) => list.Length != 0;

    public static Range indexRange<A>(this ImmutableArray<A> coll) =>
      new Range(0, coll.Length - 1);
  }

  public static class ImmutableArrayBuilderExts {
    public static ImmutableArray<A>.Builder addAnd<A>(
      this ImmutableArray<A>.Builder b, A a
    ) {
      b.Add(a);
      return b;
    }

    public static ImmutableArray<A>.Builder addOptAnd<A>(
      this ImmutableArray<A>.Builder b, Option<A> aOpt
    ) {
      if (aOpt.isSome) b.Add(aOpt.__unsafeGet);
      return b;
    }

    public static ImmutableArray<A>.Builder addRangeAnd<A>(
      this ImmutableArray<A>.Builder b, IEnumerable<A> aEnumerable
    ) {
      b.AddRange(aEnumerable);
      return b;
    }

    /// <summary>
    /// Builder throws an exception if capacity != count, this equalizes capacity before move.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="b"></param>
    /// <returns></returns>
    public static ImmutableArray<A> MoveToImmutableSafe<A>(
      this ImmutableArray<A>.Builder b
    ) => b.Capacity == b.Count ? b.MoveToImmutable() : b.ToImmutable();
    
    public static Option<int> indexWhere<A>(this ImmutableArray<A> list, Func<A, bool> predicate) {
      for (var idx = 0; idx < list.Length; idx++)
        if (predicate(list[idx])) return F.some(idx);
      return F.none<int>();
    }
    
    public static Option<int> indexWhere<A, B>(this ImmutableArray<A> list, B data, Func<A, B, bool> predicate) {
      for (var idx = 0; idx < list.Length; idx++)
        if (predicate(list[idx], data)) return F.some(idx);
      return F.none<int>();
    }

    public static Option<int> indexWhereReverse<A>(this ImmutableArray<A> list, Func<A, bool> predicate) {
      for (var idx = list.Length - 1; idx >= 0; idx--)
        if (predicate(list[idx])) return F.some(idx);
      return F.none<int>();
    }

    public static Option<int> indexWhereReverse<A, B>(
      this ImmutableArray<A> list, B data, Func<A, B, bool> predicate
    ) {
      for (var idx = list.Length - 1; idx >= 0; idx--)
        if (predicate(list[idx], data)) return F.some(idx);
      return F.none<int>();
    }
  }
}