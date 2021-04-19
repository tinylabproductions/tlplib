using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class RectTransformExts {
    public static void setAnchorMinX(this RectTransform t, float x) => t.anchorMin = t.anchorMin.withX(x);
    public static void setAnchorMinY(this RectTransform t, float y) => t.anchorMin = t.anchorMin.withY(y);
    public static void setAnchorMaxX(this RectTransform t, float x) => t.anchorMax = t.anchorMax.withX(x);
    public static void setAnchorMaxY(this RectTransform t, float y) => t.anchorMax = t.anchorMax.withY(y);
  }
}
