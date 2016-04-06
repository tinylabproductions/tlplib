using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class IEnumerableTestPartition {
    [Test]
    public void TestEquals() {
      Assert.Throws<InvalidOperationException>(() => {
        var s = F.list(1, 2);
        var p1 = s.partition(_ => true);
        var p2 = s.partition(_ => false);
        var isEqual = p1.Equals(p2);
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
