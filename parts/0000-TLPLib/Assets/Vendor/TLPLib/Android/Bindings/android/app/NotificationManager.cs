#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.app {
  public class NotificationManager : Binding {
    public NotificationManager(AndroidJavaObject java) : base(java) {}

    public void createNotificationChannel(NotificationChannel channel) =>
      java.Call("createNotificationChannel", channel.java);
  }
}

#endif
