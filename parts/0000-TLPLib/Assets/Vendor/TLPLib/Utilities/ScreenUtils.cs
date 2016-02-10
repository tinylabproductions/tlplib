using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ScreenUtils {
    private static float sw { get { return Screen.width; } }
    private static float sh { get { return Screen.height; } }

    /** Convert screen width percentage to absolute value. **/
    public static float pWidthToAbs(this float percentWidth) {
      return sw * percentWidth;
    }

    /** Convert screen height percentage to absolute value. **/
    public static float pHeightToAbs(this float percentHeight) {
      return sh * percentHeight;
    }

    /** Convert screen width absolute value to percentage. **/
    public static float aWidthToPerc(this float absoluteWidth) {
      return absoluteWidth / sw;
    }

    /** Convert screen height absolute value to percentage. **/
    public static float aHeightToPerc(this float absoluteHeight) {
      return absoluteHeight / sh;
    }
  }
}
