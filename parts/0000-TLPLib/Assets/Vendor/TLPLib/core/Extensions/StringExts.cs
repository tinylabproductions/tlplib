using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class StringExts {
    public static Either<string, int> parseInt(this string str) {
      if (int.TryParse(str, out var output)) return output;
      else return $"cannot parse as int: '{str}'";
    }

    public static Either<string, ushort> parseUShort(this string str) {
      if (ushort.TryParse(str, out var output)) return output;
      else return $"cannot parse as ushort: '{str}'";
    }

    public static Either<string, uint> parseUInt(this string str) {
      if (uint.TryParse(str, out var output)) return output;
      else return $"cannot parse as uint: '{str}'";
    }

    public static Either<string, long> parseLong(this string str) {
      if (long.TryParse(str, out var output)) return output;
      else return $"cannot parse as long: '{str}'";
    }

    public static Either<string, float> parseFloat(this string str) {
      if (float.TryParse(str, out var output)) return output;
      else return $"cannot parse as float: '{str}'";
    }

    public static Either<string, double> parseDouble(this string str) {
      if (double.TryParse(str, out var output)) return output;
      else return $"cannot parse as double: '{str}'";
    }

    public static Either<string, bool> parseBool(this string str) {
      if (bool.TryParse(str, out var output)) return output;
      else return $"cannot parse as bool: '{str}'";
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

    public static Either<Exception, string> fromBase64Safe(this string source, Encoding encoding = null) =>
      F.doTry(() => fromBase64(source, encoding)).toEither;

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

    public static bool isTrimmable(this string s) =>
      s.StartsWithFast(" ") || s.EndsWithFast(" ");

    public static bool nonEmpty(this string s) => s.Length != 0;

    public static Option<string> nonEmptyOpt(this string s, bool trim = false) {
      if (s == null) return F.none<string>();
      if (trim) s = s.Trim();
      return s.isEmpty() ? F.none<string>() : F.some(s);
    }

    public static string ensureStartsWith(this string s, string prefix)
      { return s.StartsWithFast(prefix) ? s : $"{prefix}{s}"; }

    public static string ensureEndsWith(this string s, string suffix)
      { return s.EndsWithFast(suffix) ? s : $"{s}{suffix}"; }

    public static string ensureDoesNotStartWith(this string s, string prefix)
      { return s.StartsWithFast(prefix) ? s.Substring(prefix.Length) : s; }

    public static string ensureDoesNotEndWith(this string s, string suffix)
      { return s.EndsWithFast(suffix) ? s.Substring(0, s.Length - suffix.Length) : s; }

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

    /**
     * string methods StartsWith, EndsWith, IndexOf ... by default use
     * StringComparison.CurrentCulture.
     *
     * That is about 30 times slower than StringComparison.Ordinal.
     */
    public static bool StartsWithFast(
      // ReSharper disable once MethodOverloadWithOptionalParameter
      this string s, string value, bool ignoreCase
    ) => ignoreCase
      ? s.StartsWith(value, StringComparison.OrdinalIgnoreCase)
      : StartsWithFast(s, value);

    /** See #StartsWithFast */
    public static bool EndsWithFast(
      // ReSharper disable once MethodOverloadWithOptionalParameter
      this string s, string value, bool ignoreCase
    ) => ignoreCase
      ? s.EndsWith(value, StringComparison.OrdinalIgnoreCase)
      : EndsWithFast(s, value);

    /** See #StartsWithFast */
    public static int IndexOfFast(
      this string s, string value, bool ignoreCase = false
    ) =>
      s.IndexOf(value, ordinalStringComparison(ignoreCase));

    /**
     * Even faster version of StartsWith taken from unity docs
     * https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity5.html
     */
    public static bool StartsWithFast(this string a, string b) {
      var aLen = a.Length;
      var bLen = b.Length;
      var ap = 0; 
      var bp = 0;
      while (ap < aLen && bp < bLen && a[ap] == b[bp]) {
        ap++;
        bp++;
      }
      return bp == bLen;
    }
    
    public static bool EndsWithFast(this string a, string b) {
      var ap = a.Length - 1;
      var bp = b.Length - 1;
      while (ap >= 0 && bp >= 0 && a[ap] == b[bp]) {
        ap--;
        bp--;
      }
      return bp < 0;
    }

    static StringComparison ordinalStringComparison(bool ignoreCase) =>
      ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public static Option<char> lastChar(this string s) =>
      s.isEmpty() ? None._ : s[s.Length - 1].some();

    /// <summary>obfuscates the string by shifting every char code in it by given amount</summary>
    public static string shiftCharValues(this string s, int shiftBy) =>
      new string(s.ToCharArray().map(c => (char) (c + shiftBy)));

    public static string indentLines(
      this string s, string indentWith, int indents = 1, bool indentFirst = true
    ) => s.Split('\n').indentLines(indentWith, indents, indentFirst).mkString("\n");

    public static IEnumerable<string> indentLines(
      this IEnumerable<string> lines, string indentWith, int indents = 1, bool indentFirst = true
    ) => lines.Select((line, idx) => {
      var indent = idx == 0 && indentFirst || idx != 0 ? indentWith.repeat(indents) : "";
      return $"{indent}{line}";
    });
    
    /// <summary>
    /// Split text into lines so that each line would be at most <see cref="charCount"/> characters long.
    /// </summary>
    [PublicAPI]
    public static void distributeText(
      this string text, int charCount, List<string> resultsTo
    ) {
      resultsTo.Clear();
      if (text.isEmpty()) {
        resultsTo.Add(text);
        return;
      }
      
      var currentIndex = 0;
      int remainderLength() => text.Length - currentIndex;
      while (currentIndex < text.Length) {
        var lineLength = Math.Min(charCount, remainderLength()); 
        switch (text.IndexOf('\n', currentIndex, lineLength)) {
          case -1:
            if (remainderLength() > charCount) {
              var lineText = text.Substring(currentIndex, lineLength);
              resultsTo.Add(lineText);
              currentIndex += lineLength;
            }
            else {
              resultsTo.Add(currentIndex == 0 ? text : text.Substring(currentIndex));
              currentIndex = text.Length;
            }
            break;
          case var idx:
            resultsTo.Add(text.Substring(currentIndex, idx - currentIndex));
            currentIndex = idx + 1;
            break;
        }
      }
    }

    [PublicAPI]
    public static List<string> distributeText(this string text, int charCount) {
      var list = new List<string>();
      text.distributeText(charCount, list);
      return list;
    }
  }
}
