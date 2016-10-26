#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.provider {
  public static class Settings {
    public static class Secure {
      static readonly AndroidJavaClass klass = new AndroidJavaClass("android.provider.Settings$Secure");

      public static string getString(ContentResolver contentResolver, string key) =>
        klass.CallStatic<string>("getString", contentResolver.java, key);

      public static string getAndroidId(ContentResolver contentResolver = null) =>
        getString(contentResolver ?? defaultContentResolver, "android_id");

      public static ContentResolver defaultContentResolver => 
        AndroidActivity.activity.applicationContext.contentResolver;
    }
  }
}
#endif