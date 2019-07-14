using com.tinylabproductions.TLPLib.Data;
using JetBrains.Annotations;
using static pzd.lib.config.Config;

namespace com.tinylabproductions.TLPLib.Configuration {
  [PublicAPI] public class ConfigU {
    public static readonly Parser<object, Range> iRangeParser =
      rangeParser(intParser, (l, u) => new Range(l, u));

    public static readonly Parser<object, FRange> fRangeParser =
      rangeParser(floatParser, (l, u) => new FRange(l, u));

    public static readonly Parser<object, URange> uRangeParser =
      rangeParser(uintParser, (l, u) => new URange(l, u));
  }
}