using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class GraphicExts {
    public static void applyAlpha(this Graphic graphic, float alpha) {
      graphic.color = graphic.color.withAlpha(alpha);
    }
  }
}
