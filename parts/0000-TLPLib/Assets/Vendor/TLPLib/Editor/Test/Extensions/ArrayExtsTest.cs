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
    public void ConcatTest1() {
      var a = new[] {1, 2, 3};
      var b = new[] {2, 3, 4};
      var c = new[] {10, 11, 12, 13};
      var d = new[] {9, 8};

      Assert.AreEqual(
        new[] { 1, 2, 3, 2, 3, 4, 10, 11, 12, 13, 9, 8 }, 
        a.concat(b, c, d)
      );
      Assert.AreEqual(
        new[] { 2, 3, 4, 1, 2, 3, 10, 11, 12, 13, 9, 8 },
        b.concat(a, c, d)
      );
    }

    [Test]
    public void ToImmutableTest() {
      var a = new[] {1, 2, 3};
      a.toImmutable(i => i * 2).shouldEqual(ImmutableArray.Create(2, 4, 6));
    }
  }
}