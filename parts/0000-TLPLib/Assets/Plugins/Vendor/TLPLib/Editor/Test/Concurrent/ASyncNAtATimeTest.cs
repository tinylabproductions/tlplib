using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class ASyncNAtATimeTest {
    [Test]
    public void Test() {
      var dict = new Dictionary<int, Promise<string>>();
      var queue = ASyncNAtATimeQueue.a(
        (int i) => Future<string>.async(p => dict[i] = p),
        2
      );

      var f0 = queue.enqueue(0);
      var f1 = queue.enqueue(1);
      queue.running.shouldEqual(2u);
      queue.queued.shouldEqual(0u);

      var f2 = queue.enqueue(2);
      queue.running.shouldEqual(2u);
      queue.queued.shouldEqual(1u);
      dict.ContainsKey(3).shouldBeFalse();

      var f3 = queue.enqueue(3);
      var f4 = queue.enqueue(4);
      queue.running.shouldEqual(2u);
      queue.queued.shouldEqual(3u);

      dict[1].complete("foo");
      f1.value.shouldBeSome("foo");
      queue.running.shouldEqual(2u);
      queue.queued.shouldEqual(2u);

      dict[2].complete("bar");
      f2.value.shouldBeSome("bar");
      queue.running.shouldEqual(2u);
      queue.queued.shouldEqual(1u);

      dict[0].complete("baz");
      f0.value.shouldBeSome("baz");
      queue.running.shouldEqual(2u);
      queue.queued.shouldEqual(0u);

      dict[3].complete("buz");
      f3.value.shouldBeSome("buz");
      queue.running.shouldEqual(1u);
      queue.queued.shouldEqual(0u);

      dict[4].complete("biz");
      f4.value.shouldBeSome("biz");
      queue.running.shouldEqual(0u);
      queue.queued.shouldEqual(0u);
    }
  }
}