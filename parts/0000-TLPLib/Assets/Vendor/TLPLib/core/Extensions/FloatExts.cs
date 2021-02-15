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

    [PublicAPI] public static byte roundToByteClamped(this float number) {
      if (number > byte.MaxValue) return byte.MaxValue;
      if (number < byte.MinValue) return byte.MinValue;
      return (byte) Mathf.RoundToInt(number);
    }

    public static bool approx0(this float number) => Mathf.Approximately(number, 0);
    
    /// <returns>Unit length vector that represents supplied angle</returns>
    public static Vector2 radiansToVector(this float angleRadians) => 
      new(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
  }
}