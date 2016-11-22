using System;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class DoubleExts {
    public static long roundToLong(this double d) => (long) Math.Round(d);
  }
}