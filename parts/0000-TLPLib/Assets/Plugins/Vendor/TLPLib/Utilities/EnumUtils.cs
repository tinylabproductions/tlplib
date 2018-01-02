using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class EnumUtils {
    public static IEnumerable<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();

    public static IEnumerable<Option<T>> GetValuesWithOption<T>() =>
      Option<T>.None.Yield().Concat(GetValues<T>().Select(F.some));
  }
}
