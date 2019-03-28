using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class IntExts {
    public static long toLong(this int i) => i;
    public static uint toUIntClamped(this int a) => a < 0 ? 0u : (uint) a;

    public static string toMinSecString(this int seconds) => $"{seconds / 60}:{seconds % 60:00}";
  }
}