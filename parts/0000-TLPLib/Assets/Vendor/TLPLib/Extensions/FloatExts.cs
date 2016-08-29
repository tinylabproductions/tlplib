using UnityEngine;

namespace Assets.Vendor.TLPLib.Extensions {
  public static class FloatExts {
    public static int RoundToInt(this float number) => Mathf.RoundToInt(number);
  }
}