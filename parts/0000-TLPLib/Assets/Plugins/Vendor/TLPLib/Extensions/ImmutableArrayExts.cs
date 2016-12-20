using System;
using System.Collections.Immutable;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ImmutableArrayExts {
    public static ImmutableArray<To> map<From, To>(
      this ImmutableArray<From> source, Fn<From, To> mapper
    ) {
      var target = ImmutableArray.CreateBuilder<To>(source.Length);
      for (var i = 0; i < source.Length; i++) target.Add(mapper(source[i]));
      return target.MoveToImmutable();
    }
  }
}