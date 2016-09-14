using System;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class StringExts {
    public static Either<string, int> parseInt(this String str) {
      int output;
      return int.TryParse(str, out output).either(
        () => "cannot parse as int: '" + str + "'", output
      );
    }

    public static Either<string, uint> parseUInt(this String str) {
      uint output;
      return uint.TryParse(str, out output).either(
        () => "cannot parse as uint: '" + str + "'", output
      );
    }

    public static Either<string, long> parseLong(this String str) {
      long output;
      return long.TryParse(str, out output).either(
        () => "cannot parse as long: '" + str + "'", output
      );
    }

    public static Either<string, float> parseFloat(this String str) {
      float output;
      return float.TryParse(str, out output).either(
        () => "cannot parse as float: '" + str + "'", output
      );
    }

    public static Either<string, double> parseDouble(this String str) {
      double output;
      return double.TryParse(str, out output).either(
        () => "cannot parse as double: '" + str + "'", output
      );
    }

    public static Either<string, bool> parseBool(this String str) {
      bool output;
      return bool.TryParse(str, out output).either(
        () => "cannot parse as bool: '" + str + "'", output
      );
    }

    public static Try<DateTime> parseDateTime(this string str) {
      try { return F.scs(DateTime.Parse(str)); }
      catch (Exception e) { return F.err<DateTime>(e); }
    }

    public static Try<Uri> parseUri(this string str) {
      try { return F.scs(new Uri(str)); }
      catch (Exception e) { return F.err<Uri>(e); }
    }

    public static string toBase64(this string source, Encoding encoding = null) {
      encoding = encoding ?? Encoding.UTF8;
      return Convert.ToBase64String(encoding.GetBytes(source));
    }

    public static string fromBase64(this string source, Encoding encoding = null) {
      encoding = encoding ?? Encoding.UTF8;
      return encoding.GetString(Convert.FromBase64String(source));
    }

    public static string trimTo(this string s, int length, bool fromRight=false) => 
      s.Length > length ? s.Substring(
        fromRight ? s.Length - length : 0, 
        length
      ) : s;

    public static string trimToRight(this string s, int length) =>
      s.trimTo(length, fromRight: true);

    /* Repeats string multiple times. */
    public static string repeat(this string s, int times) {
      if (times < 0) throw new ArgumentException($"{nameof(times)} must be >= 0, was {times}");
      if (times == 0) return "";
      if (times == 1) return s;
      var sb = new StringBuilder(s.Length * times);
      for (var idx = 0; idx < times; idx++) sb.Append(s);
      return sb.ToString();
    }

    public static bool isEmpty(this string s) { return s.Length == 0; }
    public static bool nonEmpty(this string s) { return s.Length != 0; }
    
    public static Option<string> nonEmptyOpt(this string s, bool trim = false) {
      if (s == null) return F.none<string>();
      if (trim) s = s.Trim();
      return s.isEmpty() ? F.none<string>() : F.some(s);
    }

    public static string ensureStartsWith(this string s, string prefix)
      { return s.StartsWith(prefix) ? s : $"{prefix}{s}"; }

    public static string ensureEndsWith(this string s, string suffix)
      { return s.EndsWith(suffix) ? s : $"{s}{suffix}"; }

    public static string ensureDoesNotStartWith(this string s, string prefix)
      { return s.StartsWith(prefix) ? s.Substring(prefix.Length) : s; }

    public static string ensureDoesNotEndWith(this string s, string suffix)
      { return s.EndsWith(suffix) ? s.Substring(0, s.Length - suffix.Length) : s; }

    public static string joinOpt(
      this string s, string joined, string separator = ": "
    ) => string.IsNullOrEmpty(joined) ? s : $"{s}{separator}{joined}";

    /**
     * Replaces a part of string with other string.
     * 
     * For example:
     * 
     * s = "foobar";
     * spliceIdx = 2;
     * spliceCount = 2;
     * spliceContent = "baz";
     * result = "fobazar";
     * 
     * Negative indexes index from the end (-1 = last char)
     **/
    public static string splice(
      this string s, int spliceIdx, int spliceCount, string spliceContent
    ) {
      if (spliceIdx >= s.Length) throw new ArgumentException(
        $"splice index ({spliceIdx}) >= string length ({s.Length})", nameof(spliceIdx)
      );
      if (-spliceIdx > s.Length) throw new ArgumentException(
        $"splice index ({spliceIdx}) > string length ({s.Length})", nameof(spliceIdx)
      );
      if (spliceIdx < 0) spliceIdx = s.Length + spliceIdx;

      var spliceEndIdx = spliceIdx + spliceCount;
      if (spliceEndIdx > s.Length) throw new ArgumentException(
        $"splice index ({spliceIdx}) + splice count ({spliceCount}) > string length ({s.Length})",
        nameof(spliceCount)
      );

      var sb = new StringBuilder(s.Length - spliceCount + spliceContent.Length);
      if (spliceIdx != 0) sb.Append(s.Substring(0, spliceIdx));
      sb.Append(spliceContent);
      if (spliceEndIdx + 1 != s.Length) sb.Append(s.Substring(spliceEndIdx));
      return sb.ToString();
    }

    // http://stackoverflow.com/a/18739120/935259
    public static string rot13(this string input) => 
      !string.IsNullOrEmpty(input) 
      ? new string(
        input.ToCharArray().Select(s => 
          (char) (
              s >= 97 && s <= 122 ? (s + 13 > 122 ? s - 13 : s + 13) 
            : s >= 65 && s <= 90  ? (s + 13 > 90 ? s - 13 : s + 13) 
            : s
          )
        ).ToArray()
      ) 
      : input;
  }
}
