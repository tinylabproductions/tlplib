using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ImmutableArrayExts {
    public static ImmutableArray<To> map<From, To>(
      this ImmutableArray<From> source, Fn<From, To> mapper
    ) {
      var target = ImmutableArray.CreateBuilder<To>(source.Length);
      for (var i = 0; i < source.Length; i++) target.Add(mapper(source[i]));
      return target.MoveToImmutable();
    }

    public static Option<T> get<T>(this ImmutableArray<T> list, int index) =>
      index >= 0 && index < list.Length ? F.some(list[index]) : F.none<T>();

    public static bool isEmpty<A>(this ImmutableArray<A> list) => list.Length == 0;
    public static bool nonEmpty<A>(this ImmutableArray<A> list) => list.Length != 0;
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
      if (aOpt.isSome) b.Add(aOpt.__unsafeGetValue);
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
  }
}