using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class EnumUtils {
    static readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();

    public static ImmutableList<T> GetValues<T>() {
      var type = typeof(T);
      var untyped = cache.getOrUpdate(type, () => Enum.GetValues(type).Cast<T>().ToImmutableList());
      return (ImmutableList<T>) untyped;
    }

    public static IEnumerable<Option<T>> GetValuesWithOption<T>() =>
      Option<T>.None.Yield().Concat(GetValues<T>().Select(F.some));
  }
}
