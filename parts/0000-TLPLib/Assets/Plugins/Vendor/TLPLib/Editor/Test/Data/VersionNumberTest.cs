using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data {
  public class VersionNumberTest {
    static readonly List<char> separators = F.list(VersionNumber.DEFAULT_SEPARATOR, '-');

    [Test]
    public void TestAsString() {
      foreach (var s in separators) {
        new VersionNumber(0, 0, 0, s).asString.shouldEqual($"0{s}0");
        new VersionNumber(1, 0, 0, s).asString.shouldEqual($"1{s}0");
        new VersionNumber(1, 1, 0, s).asString.shouldEqual($"1{s}1");
        new VersionNumber(1, 0, 1, s).asString.shouldEqual($"1{s}0{s}1");
        new VersionNumber(1, 1, 1, s).asString.shouldEqual($"1{s}1{s}1");
      }
    }

    [Test]
    public void TestParseString() {
      foreach (var s in separators) {
        VersionNumber.parseString("", s).isLeft.shouldBeTrue();
        VersionNumber.parseString($"1{s}1{s}1{s}1", s).isLeft.shouldBeTrue();
        VersionNumber.parseString($"a{s}1{s}1", s).isLeft.shouldBeTrue();
        VersionNumber.parseString($"1{s}b{s}1", s).isLeft.shouldBeTrue();
        VersionNumber.parseString($"1{s}1{s}c", s).isLeft.shouldBeTrue();
        VersionNumber.parseString("0", s).shouldBeRight(new VersionNumber(0, 0, 0, s));
        VersionNumber.parseString("1", s).shouldBeRight(new VersionNumber(1, 0, 0, s));
        VersionNumber.parseString($"1{s}1", s).shouldBeRight(new VersionNumber(1, 1, 0, s));
        VersionNumber.parseString($"1{s}1{s}0", s).shouldBeRight(new VersionNumber(1, 1, 0, s));
        VersionNumber.parseString($"1{s}0{s}1", s).shouldBeRight(new VersionNumber(1, 0, 1, s));
        VersionNumber.parseString($"1{s}1{s}1", s).shouldBeRight(new VersionNumber(1, 1, 1, s));
      }
    }

    [Test]
    public void TestAdd() {
      (new VersionNumber(1, 2, 3) + new VersionNumber(1, 2, 3)).shouldEqual(new VersionNumber(2, 4, 6));
      Assert.Throws<ArgumentException>(() => {
        var x = new VersionNumber(1, 2, 3, '.') + new VersionNumber(1, 2, 3, '-');
      });
    }

    [Test]
    public void TestWithSeparator() {
      new VersionNumber(1, 1, 1).withSeparator('~').shouldEqual(new VersionNumber(1, 1, 1, '~'));
    }
  }
}