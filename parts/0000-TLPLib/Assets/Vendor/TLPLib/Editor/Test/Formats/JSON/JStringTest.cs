using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Formats.JSON {
  /**
string
  ""
  " chars "
chars
  char
  char chars
char
  any-Unicode-character-
      except-"-or-\-or-
      control-character
  \"
  \\
  \/
  \b
  \f
  \n
  \r
  \t
  \u four-hex-digits 
**/
  class JStringTest : JsonParserTestBase {
    static string jstr(string s) { return $"\"{s}\""; }
    static Either<JsonParserFailure, JsonParserResult<string>> parse(string s) {
      return p(s, JString.instance);
    }

    static void failure(string s, int cursor) {
      var result = parse(s);
      foreach (var f in result.leftValue) {
        f.cursor.index.shouldEqual(header.Length + cursor);
        return;
      }
      Assert.Fail($"Expected '{s}' to fail parsing, but it didn't and the result was {result}");
    }

    static void success(string s, string expected=null, int lengthAdjustment=0) {
      expected = expected ?? s;
      parse(jstr(s)).shouldBeRight(res(header.Length + s.Length + 2 + lengthAdjustment, expected));
    }

    [Test] public void UnterminatedString() { failure("\"", 1 + footer.Length); }
    [Test] public void UnterminatedLongerString() { failure("\"hello", 6 + footer.Length); }
    [Test] public void DoesNotStartWithQuote() { failure("x\"", 0); }
    [Test] public void EmptyString() { success(""); }
    [Test] public void SingleChar() { success("a"); }
    [Test] public void MultipleChars() { success("aa bb cc edf"); }
    [Test] public void WithInvalidEscape() { success("a \\a b", "a \\a b"); }
    [Test] public void WithEscapedQuote() { success("a \\\" b", "a \" b"); }
    [Test] public void WithEscapedBackslash() { success("a \\\\ b", "a \\ b"); }
    [Test] public void WithEscapedSlash() { success("a \\/ b", "a / b"); }
    [Test] public void WithEscapedB() { success("a \\b b", "a \b b"); }
    [Test] public void WithEscapedF() { success("a \\f b", "a \f b"); }
    [Test] public void WithEscapedN() { success("a \\n b", "a \n b"); }
    [Test] public void WithEscapedR() { success("a \\r b", "a \r b"); }
    [Test] public void WithEscapedT() { success("a \\t b", "a \t b"); }
    [Test] public void WithEscapedUnicode() { success("a \\u00f8 \\u20AC b", "a ø € b"); }
    [Test] public void WithEscapedOutOfRangeUnicode() { success("a \\u20AG b", "a \\u20AG b"); }
    [Test] public void WithEscapedShortUnicode() { success("a \\u20A b", "a \\u20A b"); }
    [Test] public void WithEscapedLongUnicode() { success("a \\u00f8E \\u20AC_ b", "a øE €_ b"); }
  }
}
