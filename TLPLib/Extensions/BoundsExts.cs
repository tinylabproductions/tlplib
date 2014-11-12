using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class BoundsExts {
    public static Vector3 randomPoint(this Bounds bounds) {
      var min = bounds.min;
      var max = bounds.max;
      var x = Random.Range(min.x, max.x);
      var y = Random.Range(min.y, max.y);
      var z = Random.Range(min.z, max.z);
      return new Vector3(x, y, z);
    }
  }
}
