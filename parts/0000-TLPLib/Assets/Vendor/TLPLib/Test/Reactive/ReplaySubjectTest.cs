using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.test_framework;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Reactive {
  public class ReplaySubjectTest : TestBase {
    [Test]
    public void TestReplaying() {
      var s = new ReplaySubject<int>();

      var t1 = s.pipeToList(tracker);
      t1._1.shouldBeEmpty();
      t1._2.isSubscribed.shouldBeTrue();
      s.push(1);
      s.push(2);
      t1._1.shouldEqual(F.list(1, 2));

      var t2 = s.pipeToList(tracker);
      t2._1.shouldEqual(F.list(1, 2));
      t2._2.isSubscribed.shouldBeTrue();
      s.push(3);
      s.push(4);
      t1._1.shouldEqual(F.list(1, 2, 3, 4));
      t1._2.isSubscribed.shouldBeTrue();
      t2._1.shouldEqual(F.list(1, 2, 3, 4));
      t2._2.isSubscribed.shouldBeTrue();

      var t3 = s.pipeToList(tracker);
      t3._1.shouldEqual(F.list(1, 2, 3, 4));
      t3._2.isSubscribed.shouldBeTrue();
    }

    [Test]
    public void TestClearing() {
      var s = new ReplaySubject<int>();

      var t1 = s.pipeToList(tracker);
      s.push(1);
      s.push(2);

      s.clear();
      var t2 = s.pipeToList(tracker);
      t1._1.shouldEqual(F.list(1, 2));
      t2._1.shouldBeEmpty();

      s.push(1);
      s.push(2);

      t1._1.shouldEqual(F.list(1, 2, 1, 2));
      t2._1.shouldEqual(F.list(1, 2));
    }
  }
}