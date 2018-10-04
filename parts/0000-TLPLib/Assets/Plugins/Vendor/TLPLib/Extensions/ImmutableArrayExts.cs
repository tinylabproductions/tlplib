using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ImmutableArrayExts {
    [PublicAPI] public static ImmutableArray<To> map<From, To>(
      this ImmutableArray<From> source, Fn<From, To> mapper
    ) {
      var target = ImmutableArray.CreateBuilder<To>(source.Length);
      for (var i = 0; i < source.Length; i++) target.Add(mapper(source[i]));
      return target.MoveToImmutable();
    }

    [PublicAPI] public static Option<T> get<T>(this ImmutableArray<T> list, int index) =>
      index >= 0 && index < list.Length ? F.some(list[index]) : F.none<T>();

    [PublicAPI] public static bool isEmpty<A>(this ImmutableArray<A> list) => list.Length == 0;
    [PublicAPI] public static bool nonEmpty<A>(this ImmutableArray<A> list) => list.Length != 0;

    [PublicAPI] public static Range indexRange<A>(this ImmutableArray<A> coll) =>
      new Range(0, coll.Length - 1);

    [PublicAPI]
    public static ImmutableArray<A> ReplaceAt<A>(this ImmutableArray<A> arr, int idx, A value) {
      if (idx < 0 || idx >= arr.Length) throw new ArgumentOutOfRangeException(
        nameof(idx), idx, $"index {idx} is < 0 || >= array length {arr.Length}"
      );
      var b = ImmutableArray.CreateBuilder<A>(arr.Length);
      for (var i = 0; i < idx; i++) {
        b.Add(arr[i]);
      }
      b.Add(value);
      for (var i = idx + 1; i < arr.Length; i++) {
        b.Add(arr[i]);
      }
      return b.MoveToImmutable();
    }
  }

  public static class ImmutableArrayBuilderExts {
    [PublicAPI] public static ImmutableArray<A>.Builder addAnd<A>(
      this ImmutableArray<A>.Builder b, A a
    ) {
      b.Add(a);
      return b;
    }

    [PublicAPI] public static ImmutableArray<A>.Builder addOptAnd<A>(
      this ImmutableArray<A>.Builder b, Option<A> aOpt
    ) {
      if (aOpt.isSome) b.Add(aOpt.__unsafeGetValue);
      return b;
    }

    [PublicAPI] public static ImmutableArray<A>.Builder addRangeAnd<A>(
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
    [PublicAPI] public static ImmutableArray<A> MoveToImmutableSafe<A>(
      this ImmutableArray<A>.Builder b
    ) => b.Capacity == b.Count ? b.MoveToImmutable() : b.ToImmutable();
  }
}