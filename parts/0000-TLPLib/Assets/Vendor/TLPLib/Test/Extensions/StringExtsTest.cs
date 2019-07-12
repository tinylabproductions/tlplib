using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class StringTest : ImplicitSpecification {
    [Test] public void indentLines() => describe(() => {
      const string source = @"foo
bar
baz";

      when["first line should be indented"] = () => {
        it["should work"] = () =>
          source.indentLines(">.", indents: 3, indentFirst: true)
          .shouldEqual(@">.>.>.foo
>.>.>.bar
>.>.>.baz");
      };

      when["first line should not be indented"] = () => {
        it["should work"] = () =>
          source.indentLines(">.", indents: 3, indentFirst: false)
            .shouldEqual(@"foo
>.>.>.bar
>.>.>.baz");
      };
    });

    [Test]
    public void distributeText() => describe(() => {
      var results = new List<string>();
      const int MAX_LENGTH = 10;

      char chAt(int i) => (char) ('A' + i);
      string strAt(int i) => Enumerable.Range(0, i).Select(chAt).mkString("");
      
      for (var idx = 0; idx <= MAX_LENGTH; idx++) {
        var i = idx;
        it[$"should do nothing if text already fits (size {i})"] = () => {
          var text = strAt(i);
          text.distributeText(MAX_LENGTH, results);
          results.shouldEqualEnum(text);
        };

        it[$"should split if it does not fit (size {MAX_LENGTH + 1} + {i})"] = () => {
          var t1 = strAt(MAX_LENGTH + 1);
          var t2 = strAt(i);
          var text = t1 + t2;
          text.distributeText(MAX_LENGTH, results);
          var expected = F.list(
            t1.Substring(0, MAX_LENGTH), 
            t1[MAX_LENGTH] + t2.Substring(0, Math.Min(t2.Length, MAX_LENGTH - 1))
          );
          if (i == MAX_LENGTH) expected.Add(t2[MAX_LENGTH - 1].ToString());
          results.shouldEqualEnum(expected);
        };

        it[$"should handle newlines (size {i})"] = () => {
          const int NEWLINE_AT_FROM_END = MAX_LENGTH / 2 - 1;
          var t1 = strAt(MAX_LENGTH - NEWLINE_AT_FROM_END) + "\n" + strAt(NEWLINE_AT_FROM_END);
          var t2 = strAt(i);
          var text = t1 + t2;
          text.distributeText(MAX_LENGTH, results);
          var t1OnNextLine = t1.Substring(MAX_LENGTH - NEWLINE_AT_FROM_END + 1);
          var maxAllowedT2 = MAX_LENGTH - t1OnNextLine.Length;
          var expected = F.list(
            t1.Substring(0, MAX_LENGTH - NEWLINE_AT_FROM_END),
            t1OnNextLine + t2.Substring(0, Math.Min(t2.Length, maxAllowedT2))
          );
          if (t2.Length > maxAllowedT2) expected.Add(t2.Substring(maxAllowedT2));
          results.shouldEqualEnum(expected);
        };
      }
    });
  }

  public class StringTestNonEmptyOpt {
    [Test]
    public void WhenStringNull() {
      ((string) null).nonEmptyOpt().shouldBeNone();
      ((string) null).nonEmptyOpt(true).shouldBeNone();
    }

    [Test]
    public void WhenStringEmpty() {
      "".nonEmptyOpt().shouldBeNone();
      "".nonEmptyOpt(true).shouldBeNone();
    }

    [Test]
    public void WhenStringNonEmpty() {
      " ".nonEmptyOpt().shouldBeSome(" ");
      " ".nonEmptyOpt(true).shouldBeNone();
      "foo ".nonEmptyOpt().shouldBeSome("foo ");
      "foo ".nonEmptyOpt(true).shouldBeSome("foo");
    }
  }

  public class StringTestBase64Conversions {
    const string raw = "Aladdin:OpenSesame", encoded = "QWxhZGRpbjpPcGVuU2VzYW1l";

    [Test]
    public void toBase64() { raw.toBase64().shouldEqual(encoded); }

    [Test]
    public void fromBase64() { encoded.fromBase64().shouldEqual(raw); }
  }

  public class StringTestTrimTo {
    [Test]
    public void WhenWithinLimitFromLeft() {
      "foo".trimTo(3, fromRight: false).shouldEqual("foo");
    }

    [Test]
    public void WhenWithinLimitFromRight() {
      "foo".trimTo(3, fromRight: true).shouldEqual("foo");
    }

    [Test]
    public void WhenNotInLimitFromLeft() {
      "foobar".trimTo(3, fromRight: false).shouldEqual("foo");
    }

    [Test]
    public void WhenNotInLimitFromRight() {
      "foobar".trimTo(3, fromRight: true).shouldEqual("bar");
    }
  }

  public class StringTestRepeat {
    [Test]
    public void whenTimesNegative() {
      Assert.Throws<ArgumentException>(() => "foo".repeat(-1));
    }

    [Test]
    public void whenTimesZero() {
      "foo".repeat(0).shouldEqual("");
    }

    [Test]
    public void whenTimesPositive() {
      var s = "foo";
      s.repeat(1).shouldEqual(s);
      s.repeat(3).shouldEqual("foofoofoo");
    }
  }

  public class StringTestEmptyness {
    [Test]
    public void isEmpty() {
      "".isEmpty().shouldBeTrue();
      "f".isEmpty().shouldBeFalse();
    }

    [Test]
    public void nonEmpty() {
      "".nonEmpty().shouldBeFalse();
      "f".nonEmpty().shouldBeTrue();
    }
  }

  public class StringTestEnsureStartsWith {
    [Test]
    public void WhenStarts() {
      "foobar".ensureStartsWith("foo").shouldEqual("foobar");
    }
    [Test]
    public void WhenDoesNotStart() {
      "bar".ensureStartsWith("foo").shouldEqual("foobar");
    }
  }

  public class StringTestEnsureEndsWith {
    [Test]
    public void WhenEnds() {
      "foobar".ensureEndsWith("bar").shouldEqual("foobar");
    }
    [Test]
    public void WhenDoesNotEnd() {
      "bar".ensureEndsWith("foo").shouldEqual("barfoo");
    }
  }

  public class StringTestEnsureDoesNotStartWith {
    [Test]
    public void WhenStarts() {
      "foobar".ensureDoesNotStartWith("foo").shouldEqual("bar");
    }
    [Test]
    public void WhenDoesNotEnd() {
      "bar".ensureDoesNotStartWith("foo").shouldEqual("bar");
    }
  }

  public class StringTestEnsureDoesNotEndWith {
    [Test]
    public void WhenEnds() {
      "foobar".ensureDoesNotEndWith("bar").shouldEqual("foo");
    }
    [Test]
    public void WhenDoesNotEnd() {
      "bar".ensureDoesNotEndWith("foo").shouldEqual("bar");
    }
  }

  public class StringTestJoinOpt {
    [Test]
    public void WhenJoinedIsNull() =>
      "foo".joinOpt(null).shouldEqual("foo");

    [Test]
    public void WhenJoinedIsNotNull() =>
      "foo".joinOpt("bar", " - ").shouldEqual("foo - bar");
  }

  public class StringTestSplice {
    [Test]
    public void Normal() {
      "foobar".splice(0, 3, "baz").shouldEqual("bazbar");
      "foobar".splice(1, 2, "aa").shouldEqual("faabar");
      "foobar".splice(3, 3, "").shouldEqual("foo");
      "foobar".splice(0, 6, "pizza").shouldEqual("pizza");
    }

    [Test]
    public void NegativeIndex() {
      "foobar".splice(-3, 3, "baz").shouldEqual("foobaz");
      "foobar".splice(-1, 1, "z").shouldEqual("foobaz");
    }

    [Test]
    public void StartIdxOutOfRange() {
      Assert.Throws<ArgumentException>(() => "foobar".splice(-7, 0, ""));
      Assert.Throws<ArgumentException>(() => "foobar".splice(6, 0, ""));
      Assert.Throws<ArgumentException>(() => "foobar".splice(6, 0, ""));
    }

    [Test]
    public void CountOutOfRange() {
      Assert.Throws<ArgumentException>(() => "foobar".splice(-1, 2, ""));
      Assert.Throws<ArgumentException>(() => "foobar".splice(0, 7, ""));
      Assert.Throws<ArgumentException>(() => "foobar".splice(5, 2, ""));
    }
  }
}
