using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class ArrayExtsTest {
    [Test]
    public void AddOneTest() {
      new[] {1}.addOne(2).shouldEqual(new [] {1, 2});
    }

    [Test]
    public void TestConcatOne() {
      new [] {1, 2}.concat(new int[0]).shouldEqual(new [] {1, 2});
      new [] {1, 2}.concat(new [] {3}).shouldEqual(new [] {1, 2, 3});
      new int[0].concat(new [] {3}).shouldEqual(new [] {3});
      new int[0].concat(new int[0]).shouldEqual(new int[0]);
    }

    [Test]
    public void ConcatTestMany() {
      var a = new[] {1, 2, 3};
      var b = new[] {2, 3, 4};
      var c = new[] {10, 11, 12, 13};
      var d = new int[0];
      var e = new[] {9, 8};

      Assert.AreEqual(
        new[] { 1, 2, 3, 2, 3, 4, 10, 11, 12, 13, 9, 8 }, 
        a.concat(b, c, d, e)
      );
      Assert.AreEqual(
        new[] { 2, 3, 4, 1, 2, 3, 10, 11, 12, 13, 9, 8 },
        b.concat(a, c, d, e)
      );
    }

    [Test]
    public void SliceTest() {
      var source = new[] {0, 1, 2, 3, 4, 5};
      Assert.Throws<ArgumentOutOfRangeException>(() => source.slice(-1));
      source.slice(0).shouldEqual(source);
      for (var startIdx = 0; startIdx < source.Length; startIdx++)
        source.slice(startIdx, 0).shouldEqual(
          new int[0], 
          $"count=0 should return empty slice for {nameof(startIdx)}={startIdx}"
        );
      Assert.Throws<ArgumentOutOfRangeException>(() => source.slice(source.Length, 0));
      source.slice(1).shouldEqual(new []{1, 2, 3, 4, 5});
      source.slice(1, 2).shouldEqual(new []{1, 2});
      source.slice(1, 3).shouldEqual(new []{1, 2, 3});
      source.slice(2, 3).shouldEqual(new []{2, 3, 4});
      for (var startIdx = 0; startIdx < source.Length; startIdx++)
        source.slice(startIdx, 1).shouldEqual(new[] { startIdx });
      for (var startIdx = 0; startIdx < source.Length; startIdx++)
        Assert.Throws<ArgumentOutOfRangeException>(
          () => source.slice(startIdx, source.Length + 1 - startIdx)
        );
    }

    [Test]
    public void ToImmutableTest() {
      var a = new[] {1, 2, 3};
      a.toImmutable(i => i * 2).shouldEqual(ImmutableArray.Create(2, 4, 6));
    }
  }
}