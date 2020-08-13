using System;
using System.Collections.Immutable;
using pzd.lib.test_framework;
using NUnit.Framework;
using pzd.lib.exts;
using pzd.lib.test_framework.spec;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class ArrayExtsTest : ImplicitSpecification {
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
      a.toImmutable(i => i * 2).shouldEqualEnum(ImmutableArray.Create(2, 4, 6));
    }

    [Test]
    public void RemoveAtTest() {
      var a1 = new[] {1, 2, 3, 4};
      Assert.Throws<ArgumentOutOfRangeException>(() => a1.removeAt(-2));
      Assert.Throws<ArgumentOutOfRangeException>(() => a1.removeAt(-1));
      Assert.Throws<ArgumentOutOfRangeException>(() => a1.removeAt(a1.Length));
      Assert.Throws<ArgumentOutOfRangeException>(() => a1.removeAt(a1.Length + 1));
      a1.removeAt(0).shouldEqualEnum(2, 3, 4);
      a1.removeAt(1).shouldEqualEnum(1, 3, 4);
      a1.removeAt(2).shouldEqualEnum(1, 2, 4);
      a1.removeAt(3).shouldEqualEnum(1, 2, 3);

      var a2 = new[] {0};
      a2.removeAt(0).shouldBeEmpty();

      var a3 = new int[0];
      Assert.Throws<ArgumentOutOfRangeException>(() => a3.removeAt(0));
    }
    
    [Test]
    public void RemoveAtAndReplaceTest() {
      var a1 = new[] {1, 2, 3, 4};
      a1.removeAt(0, 1000);
      a1.shouldEqualEnum(2, 3, 4, 1000);

      var a2 = new[] {0};
      a2.removeAt(0, 1000);
      a2.shouldEqualEnum(1000);
    }

    [Test]
    public void shiftLeft() => describe(() => {
      when["shift argument is invalid"] = () => {
        var src = new int[0];
        beforeEach += () => src = new[] {1, 2, 3};
        
        it["should throw an exception"] = () => Assert.Throws(
          typeof(ArgumentException), () => src.shiftLeft((uint) src.Length)          
        );
        
        it["should not change the array state"] = () => {
          var workOn = src.clone();
          try { workOn.shiftLeft((uint) src.Length); }
          catch (ArgumentException) {}
          workOn.shouldEqualEnum(src);
        };
      };
      
      when["size = 2, shift = 0"] = () => {
        it["should not do anything"] = () => {
          var arr = new[] {1, 2};
          arr.shiftLeft(0);
          arr.shouldEqualEnum(1, 2);
        };
      };
      
      when["size = 2, shift = 1"] = () => {
        it["should move"] = () => {
          var arr = new[] {1, 2};
          arr.shiftLeft(1);
          arr.shouldEqualEnum(2, 2);
        };
      };
      
      when["size = 5, shift = 2"] = () => {
        it["should move"] = () => {
          var arr = new[] {1, 2, 3, 4, 5};
          arr.shiftLeft(2);
          arr.shouldEqualEnum(3, 4, 5, 4, 5);
        };
      };
      
      when["size = 7, shift = 3"] = () => {
        it["should move"] = () => {
          var arr = new[] {1, 2, 3, 4, 5, 6, 7};
          arr.shiftLeft(3);
          arr.shouldEqualEnum(4, 5, 6, 7, 5, 6, 7);
        };
      };
    });
  }
}