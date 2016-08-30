using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class IEnumerableTestAsString {
    [Test]
    public void TestNull() => 
      ((IEnumerable) null).asString().shouldEqual("null");

    [Test]
    public void TestList() {
      F.list(1, 2, 3).asString(newlines: true, fullClasses: false).shouldEqual("List`1[\n  1,\n  2,\n  3\n]");
      F.list(1, 2, 3).asString(newlines: false, fullClasses: false).shouldEqual("List`1[1, 2, 3]");
      F.list("1", "2", "3").asString(newlines: true, fullClasses: false).shouldEqual("List`1[\n  1,\n  2,\n  3\n]");
      F.list("1", "2", "3").asString(newlines: false, fullClasses: false).shouldEqual("List`1[1, 2, 3]");
    }

    [Test]
    public void TestNestedList() {
      F.list(F.list(1, 2), F.list(3)).asString().shouldEqual(
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
      Assert.Pass("Not Implemented Yet");
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
        var __ = p1.Equals(p2);
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
}
