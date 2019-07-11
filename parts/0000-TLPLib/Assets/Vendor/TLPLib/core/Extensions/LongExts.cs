using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class LongExts {
    public static int toIntClamped(this long l) {
      if (l > int.MaxValue) return int.MaxValue;
      if (l < int.MinValue) return int.MinValue;
      return (int) l;
    }
    
    public static uint toUIntClamped(this long l) {
      if (l > uint.MaxValue) return uint.MaxValue;
      if (l < uint.MinValue) return uint.MinValue;
      return (uint) l;
    }
  }
}