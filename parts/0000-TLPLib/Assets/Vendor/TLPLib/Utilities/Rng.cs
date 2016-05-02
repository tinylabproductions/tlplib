using com.tinylabproductions.TLPLib.Extensions;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class Rng {
    public static float range(float from, float to) {
      return (to - from) * Random.value + from;
    }

    public static bool chance(float chance) {
      return Random.value <= chance;
    }

    public static float[] distribution(uint size) {
      var arr = new float[size];
      var total = 1f;
      for (var idx = 0u; idx < size; idx++) {
        arr[idx] = range(0, total);
        total -= arr[idx];
      }
      arr[size - 1] += total;
      arr.Shuffle();
      return arr;
    }
  }
}
