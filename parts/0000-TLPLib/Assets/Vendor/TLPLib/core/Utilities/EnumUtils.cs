using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class EnumUtils {
    static readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();

    [PublicAPI] public static ImmutableHashSet<T> GetValues<T>() {
      var type = typeof(T);
      var untyped = cache.getOrUpdate(type, () => Enum.GetValues(type).Cast<T>().ToImmutableHashSet());
      return (ImmutableHashSet<T>) untyped;
    }

    [PublicAPI] public static Option<A> checkOpt<A>(A a) =>
      GetValues<A>().Contains(a) ? F.some(a) : F.none_;

    [PublicAPI] public static bool check<A>(A a) =>
      GetValues<A>().Contains(a);

    [PublicAPI] public static IEnumerable<Option<T>> GetValuesWithOption<T>() =>
      Option<T>.None.Yield().Concat(GetValues<T>().Select(F.some));
  }
}
