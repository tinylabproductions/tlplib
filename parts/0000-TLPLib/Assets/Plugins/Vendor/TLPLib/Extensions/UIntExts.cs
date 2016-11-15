namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UIntExts {
    public static uint addClamped(this uint a, int b) {
      if (b < 0 && a < -b) return uint.MinValue;
      if (b > 0 && uint.MaxValue - a < b) return uint.MaxValue;
      return (uint) (a + b);
    }
  }
}