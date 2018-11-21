#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.app;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.widget {
  public class Toast : Binding {
    public enum Duration {
      LENGTH_SHORT = 0, // 2 seconds
      LENGTH_LONG = 1 // 3.5 seconds
    }

    Toast(AndroidJavaObject java) : base(java) { }

    public static Toast create(string message, Duration duration, Activity activity = null) =>
      new Toast(
        new AndroidJavaClass("android.widget.Toast")
        .csjo("makeText", (activity ?? AndroidActivity.current).java, message, duration)
      );

    public void show() => java.Call("show");
  }
}
#endif