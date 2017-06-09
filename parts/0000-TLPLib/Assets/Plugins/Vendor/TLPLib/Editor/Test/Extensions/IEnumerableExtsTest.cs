using System;
using System.Collections;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class IEnumerableTestAsString {
    [Test]
    public void TestNull() => 
      ((IEnumerable) null).asDebugString().shouldEqual("null");

    [Test]
    public void TestList() {
      F.list(1, 2, 3).asString(newlines: true, fullClasses: false).shouldEqual("List`1[\n  1,\n  2,\n  3\n]");
      F.list(1, 2, 3).asString(newlines: false, fullClasses: false).shouldEqual("List`1[1, 2, 3]");
      F.list("1", "2", "3").asString(newlines: true, fullClasses: false).shouldEqual("List`1[\n  1,\n  2,\n  3\n]");
      F.list("1", "2", "3").asString(newlines: false, fullClasses: false).shouldEqual("List`1[1, 2, 3]");
    }

    [Test]
    public void TestNestedList() {
      F.list(F.list(1, 2), F.list(3)).asDebugString().shouldEqual(
@"List`1[
  List`1[
    1,
    2
  ],
  List`1[
    3
  ]
]"
      );
      F.list(F.list("1", "2"), F.list("3")).asString(newlines:true, fullClasses:false).shouldEqual(
@"List`1[
  List`1[
    1,
    2
  ],
  List`1[
    3
  ]
]"
      );
    }

    [Test]
    public void TestDictionary() {
      F.dict(F.t(1, 2), F.t(2, 3)).asString(newlines: true, fullClasses: false)
        .shouldEqual("Dictionary`2[\n  [1, 2],\n  [2, 3]\n]");
    }

    [Test]
    public void TestNestedDictionary() {
      // TODO: fixme
      Assert.Ignore("Not Implemented Yet");
      var dict = F.dict(
        F.t(1, F.dict(F.t(2, "2"))),
        F.t(2, F.dict(F.t(3, "3")))
      );
      dict.asString(newlines:true, fullClasses:false).shouldEqual(
@"Dictionary`2[
  [1, Dictionary`2[
    [2, 2]
  ],
  [2, Dictionary`2[
    [3, 3]
  ]
]");
    }
  }

  public class IEnumerableTestPartition {
    [Test]
    public void TestEquals() {
      Assert.Throws<InvalidOperationException>(() => {
        var s = F.list(1, 2);
        var p1 = s.partition(_ => true);
        var p2 = s.partition(_ => false);
        p1.Equals(p2).forSideEffects();
      });
    }

    [Test]
    public void Test() {
      var source = F.list(1, 2, 3, 4, 5);
      var empty = F.emptyList<int>();

      var emptyPartition = new int[] {}.partition(_ => true);
      emptyPartition.trues.shouldEqual(empty);
      emptyPartition.falses.shouldEqual(empty);

      var alwaysFalse = source.partition(_ => false);
      alwaysFalse.trues.shouldEqual(empty);
      alwaysFalse.falses.shouldEqual(source);

      var alwaysTrue = source.partition(_ => true);
      alwaysTrue.trues.shouldEqual(source);
      alwaysTrue.falses.shouldEqual(empty);

      var normal = source.partition(_ => _ <= 3);
      normal.trues.shouldEqual(F.list(1, 2, 3));
      normal.falses.shouldEqual(F.list(4, 5));
    }
  }

  public class IEnumerableTestZip {
    [Test]
    public void TestWhenEmpty() => 
      ImmutableList<int>.Empty.zip(ImmutableList<string>.Empty)
      .shouldEqual(ImmutableList<Tpl<int, string>>.Empty);

    [Test]
    public void TestWhenEqual() =>
      ImmutableList.Create(1, 2, 3).zip(ImmutableList.Create("a", "b", "c"), (a, b) => b + a)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3"));

    [Test]
    public void TestWhenLeftShorter() =>
      ImmutableList.Create(1, 2, 3).zip(ImmutableList.Create("a", "b", "c", "d", "e"), (a, b) => b + a)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3"));

    [Test]
    public void TestWhenRightShorter() =>
      ImmutableList.Create(1, 2, 3, 4, 5).zip(ImmutableList.Create("a", "b", "c"), (a, b) => b + a)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3"));
  }

  public class IEnumerableTestZipLeft {
    [Test]
    public void TestWhenEmpty() =>
      ImmutableList<int>.Empty
      .zipLeft(ImmutableList<string>.Empty, F.t, (a, idx) => F.t(a, idx.ToString()))
      .shouldEqual(ImmutableList<Tpl<int, string>>.Empty);

    [Test]
    public void TestWhenLeftEmpty() =>
      ImmutableList<int>.Empty
      .zipLeft(ImmutableList.Create("a", "b", "c"), F.t, (a, idx) => F.t(a, idx.ToString()))
      .shouldEqual(ImmutableList<Tpl<int, string>>.Empty);

    [Test]
    public void TestWhenRightEmpty() =>
      ImmutableList.Create(1, 2, 3)
      .zipLeft(ImmutableList<string>.Empty, (a, b) => a + b, (a, idx) => idx.ToString() + a)
      .shouldEqual(ImmutableList.Create("01", "12", "23"));

    [Test]
    public void TestWhenEqualLength() =>
      ImmutableList.Create(1, 2, 3)
      .zipLeft(ImmutableList.Create("a", "b", "c"), (a, b) => a + b, (a, idx) => idx.ToString() + a)
      .shouldEqual(ImmutableList.Create("1a", "2b", "3c"));

    [Test]
    public void TestWhenLeftShorter() =>
      ImmutableList.Create(1, 2, 3)
      .zipLeft(ImmutableList.Create("a", "b", "c", "d", "e"), (a, b) => b + a, (a, idx) => idx.ToString() + a)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3"));

    [Test]
    public void TestWhenRightShorter() =>
      ImmutableList.Create(1, 2, 3, 4, 5)
      .zipLeft(ImmutableList.Create("a", "b", "c"), (a, b) => b + a, (a, idx) => idx.ToString() + a)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3", "34", "45"));
  }

  public class IEnumerableTestZipRight {
    [Test]
    public void TestWhenEmpty() =>
      ImmutableList<int>.Empty
      .zipRight(ImmutableList<string>.Empty, F.t, (b, idx) => F.t(idx, b))
      .shouldEqual(ImmutableList<Tpl<int, string>>.Empty);

    [Test]
    public void TestWhenLeftEmpty() =>
      ImmutableList<int>.Empty
      .zipRight(ImmutableList.Create("a", "b", "c"), F.t, (b, idx) => F.t(idx, b))
      .shouldEqual(ImmutableList.Create(F.t(0, "a"), F.t(1, "b"), F.t(2, "c")));

    [Test]
    public void TestWhenRightEmpty() =>
      ImmutableList.Create(1, 2, 3)
      .zipRight(ImmutableList<string>.Empty, F.t, (b, idx) => F.t(idx, b))
      .shouldEqual(ImmutableList<Tpl<int,string>>.Empty);

    [Test]
    public void TestWhenEqualLength() =>
      ImmutableList.Create(1, 2, 3)
      .zipRight(ImmutableList.Create("a", "b", "c"), (a, b) => b + a, (b, idx) => b + idx)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3"));

    [Test]
    public void TestWhenLeftShorter() =>
      ImmutableList.Create(1, 2, 3)
      .zipRight(ImmutableList.Create("a", "b", "c", "d", "e"), (a, b) => b + a, (b, idx) => b + idx)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3", "d3", "e4"));

    [Test]
    public void TestWhenRightShorter() =>
      ImmutableList.Create(1, 2, 3, 4, 5)
      .zipRight(ImmutableList.Create("a", "b", "c"), (a, b) => b + a, (b, idx) => b + idx)
      .shouldEqual(ImmutableList.Create("a1", "b2", "c3"));
  }
}
