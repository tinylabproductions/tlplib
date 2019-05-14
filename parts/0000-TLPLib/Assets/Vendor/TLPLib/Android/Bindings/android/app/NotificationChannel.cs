#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.net;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.app {
  public class NotificationChannel : Binding {
    public enum Importance {
      None = 0,
      Low = 2,
      Default = 3,
      High = 4,
    }

    public enum LockScreenVisibility {
      Private = 0,
      Secret = -1,
      Public = 1,
    }

    public NotificationChannel(string id, string title, Importance importance) : base(new AndroidJavaObject(
      "android.app.NotificationChannel", id, title, (int) importance
    )) { }

    public void setDescription(string description) => java.Call("setDescription", description);
    public void enableLights(bool enableLights) => java.Call("enableLights", enableLights);
    public void enableVibration(bool enableVibration) => java.Call("enableVibration", enableVibration);
    public void setBypassDnd(bool bypassDnd) => java.Call("setBypassDnd", bypassDnd);
    public void setShowBadge(bool showBadge) => java.Call("setShowBadge", showBadge);
    public void setVibrationPattern(long[] vibrationPattern) => java.Call("setVibrationPattern", vibrationPattern);
    public void setLockscreenVisibility(LockScreenVisibility lockscreenVisibility) =>
      java.Call("setLockscreenVisibility", (int) lockscreenVisibility);
    public void setSound(Uri uri, AndroidJavaObject audioAttributes) => java.Call("setSound", uri.java, audioAttributes);

    public static AndroidJavaObject createAudioAttributes() =>
      new AndroidJavaObject("android.media.AudioAttributes$Builder")
        .cjo("setUsage", 5) // USAGE_NOTIFICATION
        .cjo("setContentType", 4) // CONTENT_TYPE_SONIFICATION
        .cjo("build");
  }
}
#endif