#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm;
using com.tinylabproductions.TLPLib.Android.Bindings.android.telephony;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  public class Context : Binding {
    const string 
      SERVICE_TELEPHONY_MANAGER = "phone";

    public Context(AndroidJavaObject java) : base(java) {}

    AndroidJavaObject getSystemService(string name) => 
      java.cjo("getSystemService", name);

    public TelephonyManager telephonyManager => 
      new TelephonyManager(getSystemService(SERVICE_TELEPHONY_MANAGER));

    public Context applicationContext => new Context(java.cjo("getApplicationContext"));

    /** Return a ContentResolver instance for your application's package. */
    public ContentResolver contentResolver => new ContentResolver(java.cjo("getContentResolver"));

    public PackageManager packageManager => new PackageManager(java.cjo("getPackageManager"));

    public string packageName => java.c<string>("getPackageName");
  }
}
#endif
      