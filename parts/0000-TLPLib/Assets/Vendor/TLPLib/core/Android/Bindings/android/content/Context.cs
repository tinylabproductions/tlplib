#if UNITY_ANDROID
using pzd.lib.functional;
using com.tinylabproductions.TLPLib.Android.Bindings.android.app;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm;
using com.tinylabproductions.TLPLib.Android.Bindings.android.os;
using com.tinylabproductions.TLPLib.Android.Bindings.android.telephony;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  [JavaBinding("android.content.Context"), PublicAPI]
  public class Context : Binding {
    const string
      SERVICE_VIBRATOR = "vibrator",
      SERVICE_TELEPHONY_MANAGER = "phone",
      NOTIFICATION_MANAGER = "notification";

    public enum SharedPreferencesMode : {
      // File creation mode: the default mode, where the created file can only be accessed by the
      // calling application (or all applications sharing the same user ID).
      MODE_PRIVATE = 0
    }

    public Context(AndroidJavaObject java) : base(java) {}

    public AndroidJavaObject getSystemService(string name) =>
      java.cjo("getSystemService", name);

    public Vibrator vibrator =>
      new Vibrator(getSystemService(SERVICE_VIBRATOR));

    public TelephonyManager telephonyManager =>
      new TelephonyManager(getSystemService(SERVICE_TELEPHONY_MANAGER));

    public NotificationManager notificationManager =>
      new NotificationManager(getSystemService(NOTIFICATION_MANAGER));

    public Context applicationContext => new Context(java.cjo("getApplicationContext"));

    /** Return a ContentResolver instance for your application's package. */
    public ContentResolver contentResolver => new ContentResolver(java.cjo("getContentResolver"));

    public PackageManager packageManager => new PackageManager(java.cjo("getPackageManager"));

    public string packageName => java.c<string>("getPackageName");

    public SharedPreferences getSharedPreferences(string name) =>
      // There are 3 modes, but 2 of them are deprecated:
      // https://developer.android.com/reference/android/content/Context.html#MODE_WORLD_READABLE
      new SharedPreferences(java.c<AndroidJavaObject>("getSharedPreferences", name, 0));
    
    /// <summary>
    /// https://developer.android.com/reference/android/content/Context.html#startActivity(android.content.Intent)
    /// </summary>
    public Try<Unit> startActivity(Intent intent) {
      try {
        java.Call("startActivity", intent.java);
        return F.scs(F.unit);
      }
      catch (AndroidJavaException e) {
        // might throw ActivityNotFoundException
        return F.err<Unit>(e);
      }
    }
  }
}
#endif
