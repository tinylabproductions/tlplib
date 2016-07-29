#if !ENABLE_IL2CPP
using com.tinylabproductions.TLPLib.Collection;
using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Iter;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Iter {
[TestFixture]
public class IterExtsTest {
  readonly List<int> list = F.list(1, 2, 3, 4);
  readonly LinkedList<int> linked = F.linkedList(1, 2, 3, 4);
  readonly ReadOnlyLinkedList<int> roLinked = ReadOnlyLinkedList.a(F.linkedList(1, 2, 3, 4));
  private const int elem = 3;

#region range

  private static void rangeTest(List<int> expected, Iter<int, Tpl<int, int, int>> iter) {
    Assert.AreEqual(expected, iter.toList());
    Assert.AreEqual(F.some(expected.Count), iter.elementsLeft);
  }

  private static void rangeTest(List<int> expected, Iter<int, Tpl<Iter<int, Tpl<int, int, int>>, int, int>> iter) {
    Assert.AreEqual(expected, iter.toList());
    Assert.AreEqual(F.some(expected.Count), iter.elementsLeft);
  }

  [Test] public void rangeIncrementingEmptyTest() {
    rangeTest(F.emptyList<int>(), Iter.range(5, 1, 1));
  }

  [Test] public void rangeIncrementingStep0Test() {
    var iter = Iter.range(1, 5, 0);
    Assert.AreEqual(F.none<int>(), iter.elementsLeft);
    rangeTest(F.list(1, 1, 1), Iter.range(1, 5, 0).take(3));
  }

  [Test] public void rangeIncrementingStep1Test() {
    rangeTest(F.list(1, 2, 3, 4, 5), Iter.range(1, 5));
  }

  [Test] public void rangeIncrementingStep2Test() {
    rangeTest(F.list(1, 3, 5), Iter.range(1, 5, 2));
  }

  [Test] public void rangeIncrementingStep3Test() {
    rangeTest(F.list(1, 4), Iter.range(1, 5, 3));
  }

  [Test] public void rangeDecrementingEmptyTest() {
    rangeTest(F.emptyList<int>(), Iter.range(1, 5, -1));
  }

  [Test] public void rangeDecrementingStep0Test() {
    var iter = Iter.range(1, -5, 0);
    Assert.AreEqual(F.none<int>(), iter.elementsLeft);
    rangeTest(F.list(1, 1, 1), iter.take(3));
  }

  [Test] public void rangeDecrementingStep1Test() {
    rangeTest(F.list(5, 4, 3, 2, 1), Iter.range(5, 1, -1));
  }

  [Test] public void rangeDecrementingStep2Test() {
    rangeTest(F.list(5, 3, 1), Iter.range(5, 1, -2));
  }

  [Test] public void rangeDecrementingStep3Test() {
    rangeTest(F.list(5, 2), Iter.range(5, 1, -3));
  }

  #endregion

#region IList

  [Test]
  public void listIterTest() {
    var actual = new List<int>();
    var i = list.iter();
    Assert.AreEqual(F.some(list.Count), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(list, actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void listReverseIterTest() {
    var actual = new List<int>();
    var i = list.rIter();
    Assert.AreEqual(F.some(list.Count), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(list.reversed(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void listSkippedIterTest() {
    var actual = new List<int>();
    var i = list.iter();
    Assert.AreEqual(F.some(list.Count), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(list.Count - 1), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(list.Count - 2), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(list.Skip(2).ToList(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void listSkippedReverseIterTest() {
    var actual = new List<int>();
    var i = list.rIter();
    Assert.AreEqual(F.some(list.Count), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(list.Count - 1), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(list.Count - 2), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(Enumerable.Reverse(list).Skip(2).ToList(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  #endregion

#region LinkedList

  [Test]
  public void linkedIterTest() {
    var actual = new List<int>();
    var i = linked.iter();
    Assert.AreEqual(F.some(linked.Count), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(linked, actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void linkedReverseIterTest() {
    var actual = new List<int>();
    var i = linked.rIter();
    Assert.AreEqual(F.some(linked.Count), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(linked.Reverse(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void linkedSkippedIterTest() {
    var actual = new List<int>();
    var i = linked.iter();
    Assert.AreEqual(F.some(linked.Count), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(linked.Count - 1), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(linked.Count - 2), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(linked.Skip(2).ToList(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void linkedSkippedReverseIterTest() {
    var actual = new List<int>();
    var i = linked.rIter();
    Assert.AreEqual(F.some(linked.Count), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(linked.Count - 1), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(linked.Count - 2), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(linked.Reverse().Skip(2).ToList(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void roLinkedIterTest() {
    var actual = new List<int>();
    var i = roLinked.iter();
    Assert.AreEqual(F.some(roLinked.Count), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(roLinked, actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void roLinkedReverseIterTest() {
    var actual = new List<int>();
    var i = roLinked.rIter();
    Assert.AreEqual(F.some(roLinked.Count), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(roLinked.Reverse(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void roLinkedSkippedIterTest() {
    var actual = new List<int>();
    var i = roLinked.iter();
    Assert.AreEqual(F.some(roLinked.Count), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(roLinked.Count - 1), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(roLinked.Count - 2), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(roLinked.Skip(2).ToList(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void roLinkedSkippedReverseIterTest() {
    var actual = new List<int>();
    var i = roLinked.rIter();
    Assert.AreEqual(F.some(roLinked.Count), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(roLinked.Count - 1), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(roLinked.Count - 2), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(roLinked.Reverse().Skip(2).ToList(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  #endregion

#region IEnumerable

  [Test]
  public void enumerableIterTest() {
    var actual = new List<int>();
    var i = list.hIter();
    Assert.AreEqual(F.none<int>(), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(list, actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void enumerableSkippedIterTest() {
    var actual = new List<int>();
    var i = list.hIter();
    Assert.AreEqual(F.none<int>(), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.none<int>(), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.none<int>(), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(list.Skip(2).ToList(), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  #endregion

#region Single Element

  [Test]
  public void singleElementIterTest() {
    var actual = new List<int>();
    var i = elem.singleIter();
    Assert.AreEqual(F.some(1), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.AreEqual(F.list(elem), actual);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  [Test]
  public void singleElementSkippedIterTest() {
    var actual = new List<int>();
    var i = elem.singleIter();
    Assert.AreEqual(F.some(1), i.elementsLeft);
    i.progress();
    Assert.AreEqual(F.some(0), i.elementsLeft);
    for (; i; i++) actual.Add(~i);
    Assert.True(actual.Count == 0);
    Assert.AreEqual(F.some(0), i.elementsLeft);
  }

  #endregion
}
}
#endif