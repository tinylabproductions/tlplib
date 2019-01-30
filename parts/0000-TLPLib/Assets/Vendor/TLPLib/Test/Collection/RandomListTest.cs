using System;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Collection {
  [TestFixture]
  class RandomListTest {
    #region #Insert

    [Test] public void InsertStartTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      l.Insert(0, 10);
      Assert.AreEqual(F.list(10, 2, 3, 4, 1), l);
    }

    [Test] public void InsertMiddleTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      l.Insert(2, 10);
      Assert.AreEqual(F.list(1, 2, 10, 4, 3), l);
    }

    [Test] public void InsertEndTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      l.Insert(4, 10);
      Assert.AreEqual(F.list(1, 2, 3, 4, 10), l);
    }

    [Test] public void InsertOverEndTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      Assert.Throws<ArgumentOutOfRangeException>(() => l.Insert(5, 10));
    }

    #endregion

    #region #Remove

    [Test] public void RemoveFromStartTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      Assert.AreEqual(true, l.Remove(1));
      Assert.AreEqual(F.list(4, 2, 3), l);
      Assert.AreEqual(3, l.Count);
    }

    [Test] public void RemoveFromMiddleTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      Assert.AreEqual(true, l.Remove(2));
      Assert.AreEqual(F.list(1, 4, 3), l);
      Assert.AreEqual(3, l.Count);
    }

    [Test] public void RemoveFromEndTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      Assert.AreEqual(true, l.Remove(4));
      Assert.AreEqual(F.list(1, 2, 3), l);
      Assert.AreEqual(3, l.Count);
    }

    [Test] public void RemoveNonExistingTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      Assert.AreEqual(false, l.Remove(5));
      Assert.AreEqual(F.list(1, 2, 3, 4), l);
      Assert.AreEqual(4, l.Count);
    }

    [Test] public void RemoveDuplicateTest() {
      var l = new RandomList<int> {1, 2, 2, 3, 4};
      Assert.AreEqual(true, l.Remove(2));
      Assert.AreEqual(F.list(1, 4, 2, 3), l);
      Assert.AreEqual(4, l.Count);
      Assert.AreEqual(true, l.Remove(2));
      Assert.AreEqual(F.list(1, 4, 3), l);
      Assert.AreEqual(3, l.Count);
    }

    #endregion

    #region #RemoveAt

    [Test] public void RemoveAtFromStartTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      l.RemoveAt(0);
      Assert.AreEqual(F.list(4, 2, 3), l);
      Assert.AreEqual(3, l.Count);
    }

    [Test] public void RemoveAtFromMiddleTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      l.RemoveAt(2);
      Assert.AreEqual(F.list(1, 2, 4), l);
      Assert.AreEqual(3, l.Count);
    }

    [Test] public void RemoveAtFromEndTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      l.RemoveAt(3);
      Assert.AreEqual(F.list(1, 2, 3), l);
      Assert.AreEqual(3, l.Count);
    }

    [Test] public void RemoveAtNonExistingTest() {
      var l = new RandomList<int> {1, 2, 3, 4};
      Assert.Throws<ArgumentOutOfRangeException>(() => l.RemoveAt(5));
      Assert.AreEqual(F.list(1, 2, 3, 4), l);
      Assert.AreEqual(4, l.Count);
    }

    #endregion

    #region #RemoveWhere

    [Test] public void RemoveWhereSequentialTest() {
      var l = new RandomList<int> { 1, 2, 2, 2, 3, 4, 4, 4, 5 };
      l.RemoveWhere(i => i % 2 == 0);
      // {1, 5, 2, 2, 3, 4, 4, 4}
      // {1, 5, 4, 2, 3, 4, 4}
      // {1, 5, 4, 2, 3, 4}
      // {1, 5, 4, 2, 3}
      // {1, 5, 3, 2}
      // {1, 5, 3}
      Assert.AreEqual(F.list(1, 5, 3), l);
      Assert.AreEqual(3, l.Count);
    }

    [Test] public void RemoveWhereAlteringTest() {
      var l = new RandomList<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      l.RemoveWhere(i => i % 2 == 0);
      // { 1, 9, 3, 4, 5, 6, 7, 8 }
      // { 1, 9, 3, 8, 5, 6, 7 }
      // { 1, 9, 3, 7, 5, 6 }
      // { 1, 9, 3, 7, 5 }
      Assert.AreEqual(F.list(1, 9, 3, 7, 5), l);
      Assert.AreEqual(5, l.Count);
    }

    [Test] public void RemoveWhereAllTest() {
      var l = new RandomList<int> { 2, 4, 6, 8, 10 };
      l.RemoveWhere(i => i % 2 == 0);
      Assert.AreEqual(F.emptyList<int>(), l);
      Assert.AreEqual(0, l.Count);
    }

    [Test] public void RemoveWhereNoneTest() {
      var l = new RandomList<int> { 1, 3, 5, 7, 9 };
      l.RemoveWhere(i => i % 2 == 0);
      Assert.AreEqual(F.list(1, 3, 5, 7, 9), l);
      Assert.AreEqual(5, l.Count);
    }

    #endregion
  }
}