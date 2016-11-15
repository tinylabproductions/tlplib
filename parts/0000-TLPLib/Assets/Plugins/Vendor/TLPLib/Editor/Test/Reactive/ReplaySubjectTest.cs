using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Reactive {
  public class ReplaySubjectTest {
    [Test]
    public void TestReplaying() {
      var s = new ReplaySubject<int>();

      var t1 = s.pipeToList();
      t1._1.shouldBeEmpty();
      t1._2.isSubscribed.shouldBeTrue();
      s.push(1);
      s.push(2);
      t1._1.shouldEqual(F.list(1, 2));

      var t2 = s.pipeToList();
      t2._1.shouldEqual(F.list(1, 2));
      t2._2.isSubscribed.shouldBeTrue();
      s.push(3);
      s.push(4);
      t1._1.shouldEqual(F.list(1, 2, 3, 4));
      t1._2.isSubscribed.shouldBeTrue();
      t2._1.shouldEqual(F.list(1, 2, 3, 4));
      t2._2.isSubscribed.shouldBeTrue();

      var t3 = s.pipeToList();
      t3._1.shouldEqual(F.list(1, 2, 3, 4));
      t3._2.isSubscribed.shouldBeTrue();

      s.finish();
      t1._2.isSubscribed.shouldBeFalse();
      t2._2.isSubscribed.shouldBeFalse();
      t3._2.isSubscribed.shouldBeFalse();

      Assert.Throws<ObservableFinishedException>(() => s.push(-1));

      var t4 = s.pipeToList();
      t4._1.shouldEqual(F.list(1, 2, 3, 4));
      t4._2.isSubscribed.shouldBeFalse();
    }

    [Test]
    public void TestClearing() {
      var s = new ReplaySubject<int>();

      var t1 = s.pipeToList();
      s.push(1);
      s.push(2);

      s.clear();
      var t2 = s.pipeToList();
      t1._1.shouldEqual(F.list(1, 2));
      t2._1.shouldBeEmpty();

      s.push(1);
      s.push(2);

      t1._1.shouldEqual(F.list(1, 2, 1, 2));
      t2._1.shouldEqual(F.list(1, 2));

      s.finish();
      s.finished.shouldBeTrue();
      s.clear();

      var t3 = s.pipeToList();
      t3._1.shouldBeEmpty();
      t3._2.isSubscribed.shouldBeFalse();
    }
  }
}