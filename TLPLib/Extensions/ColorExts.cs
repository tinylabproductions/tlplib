using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ColorExts {
    public static Color with(
      this Color color, float r=-1, float g=-1, float b=-1, float a=-1
    ) {
      return new Color(
        r < 0 ? color.r : r,
        g < 0 ? color.g : g,
        b < 0 ? color.b : b,
        a < 0 ? color.a : a
      );
    }

    public static Color withAlpha(this Color color, float alpha) {
      return color.with(a: alpha);
    }

    public static Color32 with32(
      this Color32 color, int r=-1, int g=1, int b=-1, int a=-1
    ) {
      return new Color(
        r < 0 ? color.r : r,
        g < 0 ? color.g : g,
        b < 0 ? color.b : b,
        a < 0 ? color.a : a
      );
    }

    public static Color32 with32Alpha(this Color32 color, int alpha) {
      return color.with32(a: alpha);
    }
  }
}
