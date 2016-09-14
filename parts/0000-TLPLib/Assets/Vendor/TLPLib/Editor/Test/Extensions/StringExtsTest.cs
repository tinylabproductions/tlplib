using System;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
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
