using UnityEngine;

namespace Assets.Vendor.TLPLib.Extensions {
  public static class FloatExts {
    public static int roundToInt(this float number) => Mathf.RoundToInt(number);
  }
}