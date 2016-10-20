#if !ENABLE_IL2CPP
using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Iter;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Iter {
[TestFixture]
public class IterTest {
  readonly List<int> list = F.list(1, 2, 3, 4, 5, 6, 7, 8);

#region general interface

  [Test]
  public void operatorsTest() {
    var iter = F.list(1, 2, 3).iter();
    var ppIter = iter;
    Assert.AreEqual(1, ~iter);
    Assert.AreEqual(1, ~ppIter);
    iter.progress();
    Assert.AreEqual(2, ~iter);
    Assert.AreEqual(1, ~(ppIter++));
    Assert.AreEqual(2, ~ppIter);
    iter.progress();
    Assert.AreEqual(3, ~iter);
    Assert.AreEqual(3, ~(++ppIter));
    iter.progress();
    Assert.AreEqual(F.none<int>(), iter.current);
    Assert.AreEqual(F.none<int>(), (++ppIter).current);
  }

#endregion

#region toList

  [Test]
  public void toListTest() {
    var actual = list.iter().toList();
    Assert.AreEqual(list.Count, actual.Capacity);
    Assert.AreEqual(list, actual);
  }

  [Test]
  public void toListSkippedTest() {
    var i = list.iter();
    i++;
    i++;
    var actual = i.toList();
    Assert.AreEqual(list.Count - 2, actual.Capacity);
    Assert.AreEqual(list.Skip(2).ToList(), actual);
  }

  [Test]
  public void toReveredListTest() {
    var actual = list.rIter().toList();
    Assert.AreEqual(list.Count, actual.Capacity);
    Assert.AreEqual(list.reversed(), actual);
  }

  [Test]
  public void toReverseListSkippedTest() {
    var i = list.rIter();
    i++;
    i++;
    var actual = i.toList();
    Assert.AreEqual(list.Count - 2, actual.Capacity);
    Assert.AreEqual(list.reversed().Skip(2).ToList(), actual);
  }

#endregion

#region toArray

  [Test]
  public void toArrayTest() {
    var actual = list.iter().toArray();
    Assert.AreEqual(list.Count, actual.Length);
    Assert.AreEqual(list, actual);
  }

  [Test]
  public void toArraySkippedTest() {
    var i = list.iter();
    i++;
    i++;
    var actual = i.toArray();
    Assert.AreEqual(list.Count - 2, actual.Length);
    Assert.AreEqual(list.Skip(2).ToArray(), actual);
  }

  [Test]
  public void toReveredArrayTest() {
    var actual = list.rIter().toArray();
    Assert.AreEqual(list.Count, actual.Length);
    Assert.AreEqual(list.reversed(), actual);
  }

  [Test]
  public void toReverseArraySkippedTest() {
    var i = list.rIter();
    i++;
    i++;
    var actual = i.toArray();
    Assert.AreEqual(list.Count - 2, actual.Length);
    Assert.AreEqual(list.reversed().Skip(2).ToArray(), actual);
  }

#endregion

#region toLinkedList

  [Test]
  public void toLinkedListTest() {
    var actual = list.iter().toLinkedList();
    Assert.AreEqual(list.Count, actual.Count);
    Assert.AreEqual(new LinkedList<int>(list), actual);
  }

  [Test]
  public void toLinkedListSkippedTest() {
    var i = list.iter();
    i++;
    i++;
    var actual = i.toLinkedList();
    Assert.AreEqual(list.Count - 2, actual.Count);
    Assert.AreEqual(new LinkedList<int>(list.Skip(2)), actual);
  }

  [Test]
  public void toReverseLinkedListTest() {
    var actual = list.rIter().toLinkedList();
    Assert.AreEqual(list.Count, actual.Count);
    Assert.AreEqual(new LinkedList<int>(list.reversed()), actual);
  }

  [Test]
  public void toReverseLinkedListSkippedTest() {
    var i = list.rIter();
    i++;
    i++;
    var actual = i.toLinkedList();
    Assert.AreEqual(list.Count - 2, actual.Count);
    Assert.AreEqual(new LinkedList<int>(list.reversed().Skip(2)), actual);
  }

#endregion

#region skip

  [Test]
  public void skipZeroTest() {
    Assert.AreEqual(list, list.iter().skip(0).toList());
  }

  [Test]
  public void skipSomeTest() {
    const int N = 2;
    Assert.AreEqual(list.Skip(N).ToList(), list.iter().skip(N).toList());
  }

  [Test]
  public void skipDoNotMutateTest() {
    var iter = list.iter();
    list.iter().skip(3);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

  [Test]
  public void skipAllTest() {
    var N = list.Count;
    Assert.AreEqual(list.Skip(N).ToList(), list.iter().skip(N).toList());
  }

  [Test]
  public void skipAllAndMoreTest() {
    var N = list.Count + 3;
    Assert.AreEqual(F.emptyList<int>(), list.iter().skip(N).toList());
  }

#endregion

#region take

  [Test]
  public void takeZeroTest() {
    Assert.AreEqual(F.emptyList<int>(), list.iter().take(0).toList());
  }

  [Test]
  public void takeSomeTest() {
    const int N = 2;
    Assert.AreEqual(list.Take(N).ToList(), list.iter().take(N).toList());
  }

  [Test]
  public void takeAllTest() {
    var N = list.Count;
    Assert.AreEqual(list, list.iter().take(N).toList());
  }

  [Test]
  public void takeDoNotMutateTest() {
    var iter = list.iter();
    list.iter().take(3);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

  [Test]
  public void takeAllAndMoreTest() {
    var N = list.Count + 3;
    Assert.AreEqual(list, list.iter().take(N).toList());
  }

  [Test]
  public void takeSizeHintTest() {
    Assert.AreEqual(F.some(1), list.iter().take(1).elementsLeft);
    Assert.AreEqual(F.some(2), list.iter().take(2).elementsLeft);
    Assert.AreEqual(F.some(3), list.iter().take(3).elementsLeft);
    Assert.AreEqual(F.some(list.Count), list.iter().take(list.Count + 10).elementsLeft);
  }

#endregion

#region zip

  [Test]
  public void zipTestBothAreEmpty() {
    Assert.AreEqual(
      F.emptyList<Tpl<int, int>>(),
      F.emptyList<int>().iter().zip(F.emptyList<int>().iter()).toList()
    );
  }

  [Test]
  public void zipTestFirstIsEmpty() {
    Assert.AreEqual(
      F.emptyList<Tpl<int, int>>(),
      F.list(1, 2, 3).iter().zip(F.emptyList<int>().iter()).toList()
    );
  }

  [Test]
  public void zipTestSecondIsEmpty() {
    Assert.AreEqual(
      F.emptyList<Tpl<int, int>>(),
      F.emptyList<int>().iter().zip(F.list(1, 2, 3).iter()).toList()
    );
  }

  [Test]
  public void zipTestFirstIsShorter() {
    Assert.AreEqual(
      F.list(F.t(1, 1f)),
      F.list(1).iter().zip(F.list(1f, 2f, 3f).iter()).toList()
    );
    Assert.AreEqual(
      F.list(F.t(1, 1f), F.t(2, 2f)),
      F.list(1, 2).iter().zip(F.list(1f, 2f, 3f).iter()).toList()
    );
  }

  [Test]
  public void zipTestSecondIsShorter() {
    Assert.AreEqual(
      F.list(F.t(1, 1f), F.t(2, 2f)),
      F.list(1, 2, 3).iter().zip(F.list(1f, 2f).iter()).toList()
    );
  }

  [Test]
  public void zipTestBothAreEqual() {
    Assert.AreEqual(
      F.list(F.t(1, 1f), F.t(2, 2f), F.t(3, 3f)),
      F.list(1, 2, 3).iter().zip(F.list(1f, 2f, 3f).iter()).toList()
    );
  }

  [Test]
  public void zipDoNotMutateTest() {
    var iter = list.iter();
    iter.zip(list.iter());
    Assert.AreEqual(list, iter.toList());
  }

#endregion

#region zipWithIndex

  [Test]
  public void zipWithIndexTest() {
    var actual = list.iter().zipWithIndex().toList();
    var idx = -1;
    Assert.AreEqual(list.Select(i => {
      idx++;
      return F.t(i, idx);
    }), actual);
  }

  [Test]
  public void zipWithIndexSkippedTest() {
    const int N = 2;
    var actual = list.iter().skip(N).zipWithIndex().toList();
    var idx = -1;
    Assert.AreEqual(list.Skip(N).Select(i => {
      idx++;
      return F.t(i, idx);
    }), actual);
  }

  [Test]
  public void zipWithIndexDoNotMutateTest() {
    var iter = list.iter();
    iter.zipWithIndex().toList();
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region each

  [Test]
  public void eachTest() {
    var actual = F.emptyList<int>();
    list.iter().each(actual.Add);
    Assert.AreEqual(list, actual);
  }

  [Test]
  public void eachDoNotMutateTest() {
    var i = list.iter();
    i.each(_ => {});
    Assert.AreEqual(list, i.toList()); // Do not mutate Iter.
  }

#endregion

#region eachWithIndex

  [Test]
  public void eachWithIndexTest() {
    var actual = F.emptyList<Tpl<int, int>>();
    list.iter().eachWithIndex((a, i) => actual.Add(F.t(a, i)));
    var idx = -1;
    Assert.AreEqual(list.Select(i => {
      idx++;
      return F.t(i, idx);
    }), actual);
  }

  [Test]
  public void eachWithIndexDoNotMutateTest() {
    var i = list.iter();
    i.eachWithIndex((a, b) => { });
    Assert.AreEqual(list, i.toList()); // Do not mutate Iter.
  }

#endregion

#region filter

  [Test]
  public void filterEmptyTest() {
    Assert.AreEqual(
      F.emptyList<int>(), F.emptyList<int>().iter().filter(_ => true).toList()
    );
  }

  [Test]
  public void filterAllFailTest() {
    Assert.
      AreEqual(F.emptyList<int>(), list.iter().filter(_ => false).toList());
  }

  [Test]
  public void filterAllPassTest() {
    Assert.AreEqual(list, list.iter().filter(_ => true).toList());
  }

  [Test]
  public void filterSomePassTest() {
    Assert.AreEqual(
      F.list(1, 3, 5), 
      F.list(1, 2, 3, 4, 5, 6).iter().filter(i => i % 2 != 0).toList()
    );
  }

  [Test]
  public void filterFailInARowTest() {
    Assert.AreEqual(
      F.list(1, 6), 
      F.list(1, 2, 3, 4, 5, 6).iter().filter(i => i < 2 || i > 5).toList()
    );
  }

  [Test]
  public void filterDoNotMutateTest() {
    var iter = list.iter();
    iter.filter(_ => true);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region map

  [Test]
  public void mapTestEmpty() {
    Assert.AreEqual(
      F.emptyList<string>(), 
      F.emptyList<int>().iter().map(_ => _.ToString()).toList()
    );
  }

  [Test]
  public void mapTestNonEmpty() {
    Assert.AreEqual(
      F.list("1", "2", "3"), 
      F.list(1, 2, 3).iter().map(_ => _.ToString()).toList()
    );
  }

  [Test]
  public void mapTestDoesNotMutate() {
    var iter = list.iter();
    iter.map(_ => _.ToString());
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region flatMap

  [Test]
  public void flatMapTestBothEmpty() {
    Assert.AreEqual(
      F.emptyList<int>(), 
      F.emptyList<int>().iter().
        flatMap(_ => F.emptyList<int>().iter()).toList()
    );
  }

  [Test]
  public void flatMapTestFirstIsEmpty() {
    Assert.AreEqual(
      F.emptyList<int>(), 
      F.emptyList<int>().iter().flatMap(_ => F.list(_, _).iter()).toList()
    );
  }

  [Test]
  public void flatMapTestSecondIsEmpty() {
    Assert.AreEqual(
      F.emptyList<int>(),
      F.list(1, 2, 3).iter().flatMap(_ => F.emptyList<int>().iter()).toList()
    );
  }

  [Test]
  public void flatMapTestSecondIsSometimesEmpty() {
    Assert.AreEqual(
      F.list(0, 2, 2, 4, 4, 6),
      F.list(1, 2, 3, 4, 5).iter().flatMap(i => 
        (i % 2 == 0 ? F.emptyList<int>() : F.list(i - 1, i + 1)).iter()
      ).toList()
    );
  }

  [Test]
  public void flatMapTestSecondTwoInARowEmpty() {
    Assert.AreEqual(
      F.list(0, 2, 4, 6),
      F.list(1, 2, 3, 4, 5).iter().flatMap(i => 
        (i >= 2 && i <= 4 ? F.emptyList<int>() : F.list(i - 1, i + 1)).iter()
      ).toList()
    );
  }

  [Test]
  public void flatMapTestNonEmpty() {
    Assert.AreEqual(
      F.list(10, 11, 20, 21, 30, 31), 
      F.list(1, 2, 3).iter().
        flatMap(i => F.list(i * 10, i * 10 + 1).iter()).toList()
    );
  }

  [Test]
  public void flatMapTestDoesNotMutate() {
    var iter = list.iter();
    iter.flatMap(_ => F.list(_, _).iter());
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region foldLeft

  [Test]
  public void foldEmptyTest() {
    var actual = F.emptyList<int>().iter().foldLeft(100, (sum, i) => sum + i);
    Assert.AreEqual(100, actual);
  }

  [Test]
  public void foldNonEmptyTest() {
    var actual = list.iter().foldLeft(0, (sum, i) => sum + i);
    Assert.AreEqual(list.Sum(), actual);
  }

  [Test]
  public void foldDoNotMutateTest() {
    var iter = list.iter();
    iter.foldLeft(0, (sum, i) => sum);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region reduceLeft

  [Test]
  public void reduceEmptyTest() {
    var actual = F.emptyList<int>().iter().reduceLeft((sum, i) => sum + i);
    Assert.AreEqual(F.none<int>(), actual);
  }

  [Test]
  public void reduceNonEmptyTest() {
    var actual = list.iter().reduceLeft((sum, i) => sum + i);
    Assert.AreEqual(F.some(list.Sum()), actual);
  }

  [Test]
  public void reduceDoNotMutateTest() {
    var iter = list.iter();
    iter.reduceLeft((sum, i) => sum);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region reduceLeft with extractor

  [Test]
  public void reduceWithExtractorEmptyTest() {
    var actual = F.emptyList<int>().iter().
      reduceLeft(_ => (float) _, (sum, i) => sum + i);
    Assert.AreEqual(F.none<float>(), actual);
  }

  [Test]
  public void reduceWithExtractorNonEmptyTest() {
    var actual = list.iter().reduceLeft(_ => (float) _, (sum, i) => sum + i);
    Assert.AreEqual(F.some(list.Select(_ => (float)_).Sum()), actual);
  }

  [Test]
  public void reduceWithExtractorDoNotMutateTest() {
    var iter = list.iter();
    iter.reduceLeft(_ => (float)_, (sum, i) => sum);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region keepLeft

  [Test]
  public void keepLeftEmptyTest() {
    var actual = F.emptyList<int>().iter().keepLeft((a, b) => a < b);
    Assert.AreEqual(F.none<int>(), actual);
  }

  [Test]
  public void keepLeftNonEmptyTest() {
    Assert.AreEqual(F.some(list.Min()), list.iter().keepLeft((a, b) => a < b));
    Assert.AreEqual(F.some(list.Max()), list.iter().keepLeft((a, b) => a > b));
  }

  [Test]
  public void keepLeftDoNotMutateTest() {
    var iter = list.iter();
    iter.keepLeft((a, b) => a < b);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region avg

  [Test]
  public void avgEmptyTest() {
    Assert.AreEqual(
      F.none<float>(), 
      F.emptyList<float>().iter().avg(_ => _)
    );
  }

  [Test]
  public void avgTest() {
    Assert.AreEqual(
      F.some(1f),
      F.list(F.t(0.5f, 1), F.t(1f, 1), F.t(1.5f, 1)).iter().avg(_ => _._1)
    );
  }

  [Test]
  public void avgDoNotMutateTest() {
    var iter = list.iter();
    iter.avg(_ => _);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region mkString

  [Test] 
  public void mkStringEmptyTest() {
    var empty = F.emptyList<int>().iter();
    Assert.AreEqual("", empty.mkString(","));
    Assert.AreEqual("><", empty.mkString(",", ">", "<"));
  }

  [Test] 
  public void mkStringNonEmptyTest() {
    var empty = F.list(1, 2, 3).iter();
    Assert.AreEqual("1,2,3", empty.mkString(","));
    Assert.AreEqual(">1,2,3<", empty.mkString(",", ">", "<"));
    Assert.AreEqual("1,2,3<", empty.mkString(",", null, "<"));
    Assert.AreEqual(">1,2,3", empty.mkString(",", ">", null));
  }

  [Test]
  public void mkStringDoNotMutateTest() {
    var iter = list.iter();
    iter.mkString(" ");
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region indexWhere

  [Test]
  public void indexWhereEmptyTest() {
    Assert.AreEqual(
      F.none<int>(), F.emptyList<int>().iter().indexWhere(_ => true)
    );
  }

  [Test]
  public void indexWhereNonEmptyTest() {
    Assert.AreEqual(
      F.some(3), F.list(1, 2, 3, 4).iter().indexWhere(i => i == 4)
    );
  }

  [Test]
  public void indexWhereDoNotMutateTest() {
    var iter = list.iter();
    iter.indexWhere(_ => false);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region randomElementByWeight

  [Test]
  public void randomElementByWeightEmptyTest() {
    Assert.AreEqual(
      F.none<int>(), 
      F.emptyList<int>().iter().randomElementByWeight(_ => _)
    );
  }

  [Test]
  public void randomElementByWeightNonEmptyTest() {
    var iter = F.list(1, 2).iter();
    Assert.AreEqual(
      F.some(1),
      iter.randomElementByWeight(i => i == 1 ? 100 : 0)
    );
    Assert.AreEqual(
      F.some(2),
      iter.randomElementByWeight(i => i == 2 ? 100 : 0)
    );
  }

  [Test]
  public void randomElementByWeightDoNotMutateTest() {
    var iter = list.iter();
    iter.randomElementByWeight(_ => _);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region exists

  [Test]
  public void existsDoNotMutateTest() {
    var iter = list.iter();
    iter.exists(_ => _ == 3);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  } 

  [Test]
  public void existsTrueTest() {
    var actual = F.list(1, 2, 3, 4).iter().exists(_ => _ == 3);
    Assert.True(actual);
  } 

  [Test]
  public void existsFalseTest() {
    var actual = F.list(1, 2, 3, 4).iter().exists(_ => _ == -1);
    Assert.False(actual);
  } 

  [Test]
  public void existsEmptyTest() {
    var actual = F.emptyList<int>().iter().exists(_ => _ == -1);
    Assert.False(actual);
  }

#endregion

#region contains

  [Test]
  public void containsDoNotMutateTest() {
    var iter = list.iter();
    iter.contains(3);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  } 

  [Test]
  public void containsTrueTest() {
    var actual = F.list(1, 2, 3, 4).iter().contains(3);
    Assert.True(actual);
  } 

  [Test]
  public void containsFalseTest() {
    var actual = F.list(1, 2, 3, 4).iter().contains(-1);
    Assert.False(actual);
  } 

  [Test]
  public void containsEmptyTest() {
    var actual = F.emptyList<int>().iter().contains(-1);
    Assert.False(actual);
  }

#endregion

#region forall

  [Test]
  public void forallDoNotMutateTest() {
    var iter = list.iter();
    iter.forall(_ => _ == 3);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  } 

  [Test]
  public void forallTrueTest() {
    var actual = F.list(1, 2, 3, 4).iter().forall(_ => _ < 5);
    Assert.True(actual);
  } 

  [Test]
  public void forallFalseTest() {
    var actual = F.list(1, 2, 3, 4).iter().forall(_ => _ == -1);
    Assert.False(actual);
  } 

  [Test]
  public void forallEmptyTest() {
    var actual = F.emptyList<int>().iter().forall(_ => _ == -1);
    Assert.True(actual);
  } 

#endregion

#region find

  [Test]
  public void findSuccessfulTest() {
    Assert.AreEqual(F.some(3), F.list(1, 2, 3, 4).iter().find(_ => _ == 3));
  }

  [Test]
  public void findUnsuccessfulTest() {
    Assert.AreEqual(F.none<int>(), F.list(1,2,3,4).iter().find(_ => _ == 0));
  }

  [Test]
  public void findDoNotMutateTest() {
    var iter = list.iter();
    iter.find(_ => false);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region find + index

  [Test]
  public void findWithIndexSuccessfulTest() {
    Assert.AreEqual(
      F.some(3), 
      F.list(1, 2, 3, 4).iter().find((a, idx) => idx == 2)
    );
  }

  [Test]
  public void findWithIndexUnsuccessfulTest() {
    Assert.AreEqual(
      F.none<int>(), 
      F.list(1,2,3,4).iter().find((a, idx) => false)
    );
  }

  [Test]
  public void findWithIndexDoNotMutateTest() {
    var iter = list.iter();
    iter.find((a, idx) => false);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region find + finder

  [Test]
  public void findFinderSuccessfulTest() {
    Assert.AreEqual(
      F.some("foo"), 
      F.list(1, 2, 3, 4).iter().find(_ => (_ == 3).opt("foo"))
    );
  }

  [Test]
  public void findFinderUnsuccessfulTest() {
    Assert.AreEqual(
      F.none<int>(),
      F.list(1, 2, 3, 4).iter().find(_ => F.none<int>())
    );
  }

  [Test]
  public void findFinderDoNotMutateTest() {
    var iter = list.iter();
    iter.find(_ => F.none<int>());
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region find + index + finder

  [Test]
  public void findWithIndexFinderSuccessfulTest() {
    Assert.AreEqual(
      F.some("foo"), 
      F.list(1, 2, 3, 4).iter().find((a, idx) => (idx == 2).opt("foo"))
    );
  }

  [Test]
  public void findWithIndexFinderUnsuccessfulTest() {
    Assert.AreEqual(
      F.none<int>(),
      F.list(1, 2, 3, 4).iter().find((a, idx) => F.none<int>())
    );
  }

  [Test]
  public void findWithIndexFinderDoNotMutateTest() {
    var iter = list.iter();
    iter.find((a, idx) => F.none<int>());
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region findOrLast

  [Test]
  public void findOrLastSuccessfulTest() {
    Assert.AreEqual(F.some(3), F.list(1, 2, 3, 4).iter().findOrLast(_ => _ == 3));
  }

  [Test]
  public void findOrLastUnsuccessfulTest() {
    Assert.AreEqual(F.some(4), F.list(1,2,3,4).iter().findOrLast(_ => false));
  }

  [Test]
  public void findOrLastEmptyTest() {
    Assert.AreEqual(F.none<int>(), F.emptyList<int>().iter().findOrLast(_ => false));
  }

  [Test]
  public void findOrLastDoNotMutateTest() {
    var iter = list.iter();
    iter.findOrLast(_ => false);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion

#region partition

  [Test]
  public void partitionEmptyTest() {
    var lists = F.emptyList<int>().iter().partition(_ => false);
    Assert.True(lists._1.isEmpty());
    Assert.True(lists._2.isEmpty());
  }

  [Test]
  public void partitionTest() {
    var lists = F.list(1, 2, 3, 4).iter().partition(_ => _ < 3);
    Assert.AreEqual(F.linkedList(1, 2), lists._1);
    Assert.AreEqual(F.linkedList(3, 4), lists._2);
  }

  [Test]
  public void partitionDoNotMutateTest() {
    var iter = list.iter();
    iter.partition(_ => true);
    Assert.AreEqual(list, iter.toList()); // Do not mutate Iter.
  }

#endregion
}
}
#endif