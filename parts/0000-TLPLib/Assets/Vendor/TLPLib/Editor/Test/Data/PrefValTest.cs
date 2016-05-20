using System;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Data {
  class PrefValTestDefaultValueStorage {
    [Test]
    public void ItShouldStoreDefaultValueUponCreation() {
      var key = $"test-{DateTime.Now}";
      var p1 = PrefVal.player.integer(key, Random.Range(0, 100));
      var p2 = PrefVal.player.integer(key, p1.value + 1);
      p2.value.shouldEqual(p1.value);
    }
  }
}
