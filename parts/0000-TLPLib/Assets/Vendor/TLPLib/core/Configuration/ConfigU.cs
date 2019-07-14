using com.tinylabproductions.TLPLib.Data;
using JetBrains.Annotations;
using pzd.lib.config;
using pzd.lib.functional;
using UnityEngine;
using static pzd.lib.config.Config;

namespace com.tinylabproductions.TLPLib.Configuration {
  [PublicAPI] public class ConfigU {
    public static readonly Parser<object, Range> iRangeParser =
      rangeParser(intParser, (l, u) => new Range(l, u));

    public static readonly Parser<object, FRange> fRangeParser =
      rangeParser(floatParser, (l, u) => new FRange(l, u));

    public static readonly Parser<object, URange> uRangeParser =
      rangeParser(uintParser, (l, u) => new URange(l, u));
    
    public static readonly Parser<object, Color> colorParser =
      stringParser.flatMap(s => ColorUtility.TryParseHtmlString(s, out var c) ? Some.a(c) : None._);
  }
}