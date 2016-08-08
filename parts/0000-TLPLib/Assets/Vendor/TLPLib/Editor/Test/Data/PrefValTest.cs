using System;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Data {
  class PrefValTestRamStorage {
    [Test]
    public void ItShouldUpdateTheValueInRam() {
      var key = $"{nameof(ItShouldUpdateTheValueInRam)}-{DateTime.Now.Ticks}";
      var p = PrefVal.player.integer(key, 100);
      p.value.shouldEqual(100);
      p.value = 200;
      p.value.shouldEqual(200);
    }
  }

  class PrefValTestDefaultValueStorage {
    [Test]
    public void ItShouldStoreDefaultValueUponCreation() {
      var key = $"{nameof(ItShouldStoreDefaultValueUponCreation)}-{DateTime.Now.Ticks}";
      var p1 = PrefVal.player.integer(key, Random.Range(0, 100));
      var p2 = PrefVal.player.integer(key, p1.value + 1);
      p2.value.shouldEqual(p1.value);
    }

    [Test]
    public void ItShouldPersistDefaultValueToPrefs() {
      var key = $"{nameof(ItShouldPersistDefaultValueToPrefs)}-{DateTime.Now.Ticks}";
      var p1 = PrefVal.player.integer(key, default(int));
      var p2 = PrefVal.player.integer(key, 10);
      p2.value.shouldEqual(p1.value);
    }
  }
}