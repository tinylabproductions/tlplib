using UnityEngine;

namespace Assets.Vendor.TLPLib.Extensions {
  public static class FloatExts {
    public static int toInt(this float number) => Mathf.RoundToInt(number);
  }
}