using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FloatExts {
    public static int roundToInt(this float number) => Mathf.RoundToInt(number);

    [PublicAPI] public static int toIntClamped(this float number) {
      if (number > int.MaxValue) return int.MaxValue;
      if (number < int.MinValue) return int.MinValue;
      return (int) number;
    }

    public static bool approx0(this float number) => Mathf.Approximately(number, 0);
  }
}