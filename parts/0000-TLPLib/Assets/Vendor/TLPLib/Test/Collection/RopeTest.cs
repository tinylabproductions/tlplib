using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Collection {
  public class RopeTest {
    [Test]
    public void TestRope() {
      var simple = Rope.create(1, 2, 3);
      simple.length.shouldEqual(3);
      simple.toArray().shouldEqual(new [] {1, 2, 3});

      var joined = Rope.create(1) + Rope.create(2, 3) + Rope.create(4, 5, 6);
      joined.length.shouldEqual(6);
      joined.toArray().shouldEqual(new [] {1, 2, 3, 4, 5, 6});

      var joinedWithArr = joined + new[] {7, 8};
      joinedWithArr.length.shouldEqual(8);
      joinedWithArr.toArray().shouldEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8 });
    }
  }
}