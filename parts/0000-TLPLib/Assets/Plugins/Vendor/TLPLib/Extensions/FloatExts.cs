using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FloatExts {
    public static int roundToInt(this float number) => Mathf.RoundToInt(number);

    [PublicAPI] public static int toIntClamped(this float number) {
      if (number > int.MaxValue) return int.MaxValue;
      if (number < int.MinValue) return int.MinValue;
      return (int) Mathf.Round(number);
    }

    [PublicAPI] public static byte toByteClamped(this float number) {
      if (number > byte.MaxValue) return byte.MaxValue;
      if (number < byte.MinValue) return byte.MinValue;
      return (byte) Mathf.Round(number);
    }

    public static bool approx0(this float number) => Mathf.Approximately(number, 0);
  }
}