using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class ASyncNAtATimeEquality : TestBase {
    [Test]
    public void OneWhenGood() {
      var queue = new ASyncNAtATimeQueue<int, int>(5, (param, promise) => { promise.complete(param); });
      queue.query(1).onComplete(s => shouldEqual(s, 1));
    }

    [Test]
    public void ALotWhenGood() {
      var maxRunners = 4;
      var addingParams = 40;
      var q = new Queue<Tpl<Promise<int>, int>>();
      var queue = new ASyncNAtATimeQueue<int, int>(maxRunners, (param, promise) => { q.Enqueue(F.t(promise, param)); });

      for (var i = 1; i <= addingParams; i++) {
        var k = i;
        queue.query(i).onComplete(fr => shouldEqual(k, fr));
      }
      shouldEqual(q.Count, maxRunners);

      while (q.Count > 0) {
        var r = q.Dequeue();
        r._1.complete(r._2);
        addingParams--;
        shouldEqual(q.Count, Math.Min(maxRunners, addingParams));
      }
    }
  }
}