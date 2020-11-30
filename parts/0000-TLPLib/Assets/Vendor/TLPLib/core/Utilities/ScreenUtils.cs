using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using pzd.lib.reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static partial class ScreenUtils {
    public static Size screenSize => new Size(Screen.width, Screen.height);

    [LazyProperty] public static IRxVal<Size> screenSizeVal => 
      ObservableU.everyFrame.map(_ => screenSize).toRxVal(screenSize);

    [LazyProperty] public static IRxVal<Rect> screenSafeArea => 
      ObservableU.everyFrame.map(_ => Screen.safeArea).toRxVal(Screen.safeArea);

    /** Convert screen width percentage to absolute value. **/
    public static float pWidthToAbs(this float percentWidth) => Screen.width * percentWidth;

    /** Convert screen height percentage to absolute value. **/
    public static float pHeightToAbs(this float percentHeight) => Screen.height * percentHeight;

    /** Convert screen width absolute value to percentage. **/
    public static float aWidthToPerc(this float absoluteWidth) => absoluteWidth / Screen.width;

    /** Convert screen height absolute value to percentage. **/
    public static float aHeightToPerc(this float absoluteHeight) => absoluteHeight / Screen.height;
  }
}
