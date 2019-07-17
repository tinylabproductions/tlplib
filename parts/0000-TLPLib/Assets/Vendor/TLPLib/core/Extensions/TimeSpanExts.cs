using System;
using System.Linq;
using System.Text.RegularExpressions;
using static pzd.lib.typeclasses.Str;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class TimeSpanExts {
    public static string toHumanString(
      this TimeSpan ts, int maxParts = int.MaxValue
    ) {
      var parts =
        $"{ts.Days:D2}d:{ts.Hours:D2}h:{ts.Minutes:D2}m:{ts.Seconds:D2}s:{ts.Milliseconds:D3}ms"
        .Split(':')
        .SkipWhile(s => Regex.Match(s, @"00\w").Success) // skip zero-valued components
        .Take(maxParts).ToArray();
      var result = string.Join(" ", parts); // combine the result

      return result;
    }

    public static string toHoursAndMinutes(this TimeSpan time) {
      var totalHours = time.Days * 24 + time.Hours;
      return totalHours > 0
              ? $"{s(totalHours)} h {s(time.Minutes)} min"
              : $"{s(time.Minutes)} min {s(time.Seconds)} s";
    }
  }
}
