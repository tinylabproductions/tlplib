namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ByteExts {
    public static byte subtractClamped(this byte b1, byte b2) => b2 > b1 ? (byte) 0 : (byte) (b1 - b2);
  }
}