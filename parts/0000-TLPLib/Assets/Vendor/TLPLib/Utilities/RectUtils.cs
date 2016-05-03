using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class RectUtils {
    /** Create rect that has values from percentages of screen. **/
    public static Rect percent(float leftP, float topP, float widthP, float heightP) {
      return new Rect(
        leftP.pWidthToAbs(), topP.pHeightToAbs(),
        widthP.pWidthToAbs(), heightP.pHeightToAbs()
      );
    }

    /** Convert absolute rect to percentage rect. **/
    public static Rect absoluteToPercentage(this Rect pRect) {
      return new Rect(
        pRect.xMin.aWidthToPerc(), pRect.yMin.aHeightToPerc(),
        pRect.width.aWidthToPerc(), pRect.height.aHeightToPerc()
      );
    }

    /** Create rect that has values from percentages of screen. **/
    public static Rect relPercent(float left, float leftEnd, float top, float topEnd) {
      return percent(left, top, leftEnd - left, topEnd - top);
    }

    public static Rect with(
      this Rect rect, float? left = null, float? top = null,
      float? width = null, float? height = null
    ) {
      return new Rect(
        left ?? rect.xMin, top ?? rect.yMin,
        width ?? rect.width, height ?? rect.height
      );
    }

    /* Scale (pivot point: center) */
    public static Rect scale(this Rect rect, float scale) {
      var newW = rect.width * scale;
      var newH = rect.height * scale;
      var wDiff = newW - rect.width;
      var hDiff = newH - rect.height;
      return new Rect(rect.xMin - wDiff / 2, rect.yMin - hDiff / 2, newW, newH);
    }
  }
}
