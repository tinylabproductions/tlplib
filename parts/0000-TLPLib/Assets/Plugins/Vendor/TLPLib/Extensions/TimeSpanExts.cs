using System;
using System.Linq;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Data.typeclasses;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class TimeSpanExts {
    public static string toHumanString(
      this TimeSpan ts, int maxParts = int.MaxValue
    ) {
      var parts = 
        string.Format(
          "{0:D2}d:{1:D2}h:{2:D2}m:{3:D2}s:{4:D3}ms",
          ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds
        )
        .Split(':')
        .SkipWhile(s => Regex.Match(s, @"00\w").Success) // skip zero-valued components
        .Take(maxParts).ToArray();
      var result = string.Join(" ", parts); // combine the result

      return result;
    }

    public static string toHoursAndMinutes(this TimeSpan time) {
      var totalHours = time.Days * 24 + time.Hours;
      return totalHours > 0
              ? $"{Str.s(totalHours)} h {Str.s(time.Minutes)} min"
              : $"{Str.s(time.Minutes)} min {Str.s(time.Seconds)} s";
    }
  }
}
