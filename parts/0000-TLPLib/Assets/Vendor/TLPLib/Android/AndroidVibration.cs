using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
#if UNITY_ANDROID
  // https://www.reddit.com/r/Unity3D/comments/4j5js7/unity_vibrate_android_device_for_custom_duration/
  public static class AndroidVibration {
    static readonly AndroidJavaObject Vibrator =
      AndroidActivity.current.getSystemService("vibrator");

    static AndroidVibration() {
      // Trick Unity into giving the App vibration permission when it builds.
      // This check will always be false, but the compiler doesn't know that.
      if (Application.isEditor) Handheld.Vibrate();
    }

    public static void vibrate(long milliseconds) => 
      Vibrator.Call("vibrate", milliseconds);

    public static void vibrate(long[] pattern, int repeat) => 
      Vibrator.Call("vibrate", pattern, repeat);
  }
#endif
}