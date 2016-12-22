namespace com.tinylabproductions.TLPLib.Extensions {
  public static class LongExts {
    public static int toIntClamped(this long l) {
      if (l > int.MaxValue) return int.MaxValue;
      if (l < int.MinValue) return int.MinValue;
      return (int) l;
    }
  }
}