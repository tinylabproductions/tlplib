using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Collection {
  public class IListDefaultImplsTest {
    const int nums = 100;

    // IList might be a mutable struct
    static void addAll<C, A>(ref C c, params A[] args) where C : IList<A> {
      foreach (var a in args) c.Add(a);
    }

    public static void testCount<C>(C c) where C : IList<int> {
      c.Clear();
      c.Count.shouldEqual(0);
      for (var num = 1; num < nums; num++) {
        c.Add(0);
        c.Count.shouldEqual(num, $"count should equal {num}");
      }
    }

    [Test]
    public void TestDefaultCount() => testCount(new List<int>());

    public static void testClear<C>(C c) where C : IList<int> {
      c.Clear();
      c.shouldBeEmpty();
      for (var i = 1; i <= 4; i++) c.Add(i);
      c.mkStringEnum().shouldEqual(F.list(1, 2, 3, 4).mkStringEnum());
      c.shouldEqual<IList<int>>(F.list(1, 2, 3, 4));
      c.Clear();
      c.shouldBeEmpty();
    }

    [Test]
    public void TestDefaultClear() => testClear(new List<int>());

    public static void testIndexOf<C>(C c) where C : IList<int> {
      c.Clear();
      c.IndexOf(0).shouldEqual(-1);
      c.Add(1);
      c.IndexOf(1).shouldEqual(0);
      c.IndexOf(-1).shouldEqual(-1);
      c.Add(2);
      c.IndexOf(1).shouldEqual(0);
      c.IndexOf(2).shouldEqual(1);
      c.IndexOf(-1).shouldEqual(-1);
      c.Add(3);
      c.IndexOf(1).shouldEqual(0);
      c.IndexOf(2).shouldEqual(1);
      c.IndexOf(3).shouldEqual(2);
      c.IndexOf(-1).shouldEqual(-1);
      c.Add(1);
      c.IndexOf(1).shouldEqual(0);
      c.IndexOf(2).shouldEqual(1);
      c.IndexOf(3).shouldEqual(2);
      c.IndexOf(-1).shouldEqual(-1);
    }

    [Test]
    public void TestDefaultIndexOf() => testIndexOf(new List<int>());

    public static void testAdd<C>(C c) where C : IList<int> {
      c.Clear();
      for (var num = 1; num < nums; num++) {
        c.Add(-num);
        c.shouldEqual<IList<int>>(F.listFill(num, i => -(i + 1)));
      }
    }

    [Test]
    public void TestDefaultAdd() => testAdd(new List<int>());

    public static void testRemoveAt<C>(C c) where C : IList<int> {
      c.Clear();
      addAll(ref c, 1, 2, 3, 4, 5, 6);
      Assert.Throws<ArgumentOutOfRangeException>(() => c.RemoveAt(-1));
      Assert.Throws<ArgumentOutOfRangeException>(() => c.RemoveAt(6));
      c.RemoveAt(0);
      c.shouldEqual<IList<int>>(F.list(2, 3, 4, 5, 6));
      c.RemoveAt(4);
      c.shouldEqual<IList<int>>(F.list(2, 3, 4, 5));
      c.RemoveAt(1);
      c.shouldEqual<IList<int>>(F.list(2, 4, 5));
      c.RemoveAt(1);
      c.shouldEqual<IList<int>>(F.list(2, 5));
      c.RemoveAt(1);
      c.shouldEqual<IList<int>>(F.list(2));
      c.RemoveAt(0);
      c.shouldEqual<IList<int>>(F.emptyList<int>());
    }

    [Test]
    public void TestDefaultRemoveAt() => testRemoveAt(new List<int>());

    public static void testContains<C>(C c) where C : IList<int> {
      c.Clear();
      c.Contains(0).shouldBeFalse();
      c.Add(1);
      c.Contains(1).shouldBeTrue();
      c.Contains(-1).shouldBeFalse();
      c.Add(2);
      c.Contains(1).shouldBeTrue();
      c.Contains(2).shouldBeTrue();
      c.Contains(-1).shouldBeFalse();
      c.Add(3);
      c.Contains(1).shouldBeTrue();
      c.Contains(2).shouldBeTrue();
      c.Contains(3).shouldBeTrue();
      c.Contains(-1).shouldBeFalse();
      c.Add(1);
      c.Contains(1).shouldBeTrue();
      c.Contains(2).shouldBeTrue();
      c.Contains(3).shouldBeTrue();
      c.Contains(-1).shouldBeFalse();
    }

    [Test]
    public void TestDefaultContains() => testContains(new List<int>());

    public static void testRemove<C>(C c) where C : IList<int> {
      c.Clear();
      c.Remove(-1).shouldBeFalse();
      addAll(ref c, 1, 2, 3, 4, 2);
      c.Remove(5).shouldBeFalse();
      c.Remove(2).shouldBeTrue();
      c.shouldEqual<IList<int>>(F.list(1, 3, 4, 2));
      c.Remove(2).shouldBeTrue();
      c.shouldEqual<IList<int>>(F.list(1, 3, 4));
      c.Remove(1).shouldBeTrue();
      c.shouldEqual<IList<int>>(F.list(3, 4));
      c.Remove(4).shouldBeTrue();
      c.shouldEqual<IList<int>>(F.list(3));
      c.Remove(3).shouldBeTrue();
      c.shouldEqual<IList<int>>(F.emptyList<int>());
      c.Remove(3).shouldBeFalse();
    }

    [Test]
    public void TestDefaultRemove() => testRemove(new List<int>());

    public static void testInsert<C>(C c) where C : IList<char> {
      c.Clear();
      Assert.Throws<ArgumentOutOfRangeException>(() => c.Insert(-1, 'c'));
      Assert.Throws<ArgumentOutOfRangeException>(() => c.Insert(1, 'c'));
      c.Insert(0, 'a');
      c.shouldEqual<IList<char>>(F.list('a'));
      c.Insert(0, 'b');
      c.shouldEqual<IList<char>>(F.list('b', 'a'));
      c.Insert(1, 'c');
      c.shouldEqual<IList<char>>(F.list('b', 'c', 'a'));
      c.Insert(3, 'd');
      c.shouldEqual<IList<char>>(F.list('b', 'c', 'a', 'd'));
      c.Insert(4, 'e');
      c.shouldEqual<IList<char>>(F.list('b', 'c', 'a', 'd', 'e'));
      c.Insert(1, 'f');
      c.shouldEqual<IList<char>>(F.list('b', 'f', 'c', 'a', 'd', 'e'));
      Assert.Throws<ArgumentOutOfRangeException>(() => c.Insert(-1, 'c'));
      Assert.Throws<ArgumentOutOfRangeException>(() => c.Insert(100, 'c'));
    }

    [Test]
    public void TestDefaultInsert() => testInsert(new List<char>());

    public static void testCopyTo<C>(C c) where C : IList<int> {
      c.Clear();
      addAll(ref c, 1, 2, 3, 4, 5);
      // ReSharper disable once AssignNullToNotNullAttribute
      Assert.Throws<ArgumentNullException>(() => c.CopyTo(null, 0));

      var arr = new int[5];
      Assert.Throws<ArgumentOutOfRangeException>(() => c.CopyTo(arr, -1));
      Assert.Throws<ArgumentException>(() => c.CopyTo(arr, 1));

      c.CopyTo(arr, 0);
      arr.shouldEqual(new [] {1, 2, 3, 4, 5});

      arr = F.arrayFill(7, _ => 0);
      c.CopyTo(arr, 2);
      arr.shouldEqual(new[] { 0, 0, 1, 2, 3, 4, 5 });

      arr = F.arrayFill(7, i => -(i + 1));
      c.CopyTo(arr, 1);
      arr.shouldEqual(new[] { -1, 1, 2, 3, 4, 5, -7 });
    }

    [Test]
    public void TestDefaultCopyTo() => testCopyTo(new List<int>());

    public static void testIndexing<C>(C c) where C : IList<int> {
      c.Clear();
      for (var num = 1; num <= nums; num++) c.Add(num);
      // ReSharper disable once UnusedVariable
      Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = c[-1]; });
      // ReSharper disable once UnusedVariable
      Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = c[nums]; });
      for (var idx = 0; idx < nums; idx++)
        c[idx].shouldEqual(idx + 1);
    }

    [Test]
    public void TestDefaultIndexing() => testIndexing(new List<int>());

    public static void testForeach<C>(C c) where C : IList<int> {
      c.Clear();
      var testList = new List<int>();
      Action test = () => {
        testList.Clear();
        testList.AddRange(c);
        c.shouldEqual<IList<int>>(testList);
      };
      for (var num = 1; num < nums; num++) {
        c.Add(num);
        test();
      }
    }

    [Test]
    public void TestDefaultForeach() => testForeach(new List<int>());
  }
}