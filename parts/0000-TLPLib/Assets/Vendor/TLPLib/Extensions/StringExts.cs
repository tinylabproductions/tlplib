using System;
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

    public static Either<Exception, DateTime> parseDateTime(this String str) {
      try {
        return F.right<Exception, DateTime>(DateTime.Parse(str));
      }
      catch (Exception e) {
        return F.left<Exception, DateTime>(e);
      }
    }

    public static string toBase64(this string source, Encoding encoding = null) {
      encoding = encoding ?? Encoding.UTF8;
      return Convert.ToBase64String(encoding.GetBytes(source));
    }

    public static string fromBase64(this string source, Encoding encoding = null) {
      encoding = encoding ?? Encoding.UTF8;
      return encoding.GetString(Convert.FromBase64String(source));
    }

    public static string trimTo(this string s, int length) { return s.Length > length ? s.Substring(0, length) : s; }

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

    public static Option<string> nonEmptyOpt(this string s)
      { return (!string.IsNullOrEmpty(s)).opt(s); }

    public static string ensureStartsWith(this string s, string prefix)
      { return s.StartsWith(prefix) ? s : $"{prefix}{s}"; }

    public static string ensureEndsWith(this string s, string suffix)
      { return s.EndsWith(suffix) ? s : $"{s}{suffix}"; }

    public static string ensureDoesNotStartWith(this string s, string prefix)
      { return s.StartsWith(prefix) ? s.Substring(prefix.Length) : s; }

    public static string ensureDoesNotEndWith(this string s, string suffix)
      { return s.EndsWith(suffix) ? s.Substring(0, s.Length - suffix.Length) : s; }
  }
}
