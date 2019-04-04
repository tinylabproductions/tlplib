namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UShortExts {
    public static byte toByteClamped(this ushort v) {
      if (v > byte.MaxValue) return byte.MaxValue;
      return unchecked((byte) v);
    }
  }
}