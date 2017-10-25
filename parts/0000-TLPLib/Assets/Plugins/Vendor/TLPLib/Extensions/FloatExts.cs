using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FloatExts {
    public static int roundToInt(this float number) => Mathf.RoundToInt(number);

    public static bool approx0(this float number) => Mathf.Approximately(number, 0);
  }
}