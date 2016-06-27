#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings {
  public class Context : Binding {
    const string 
      SERVICE_TELEPHONY_MANAGER = "phone";

    public Context(AndroidJavaObject java) : base(java) {}

    AndroidJavaObject getSystemService(string name) => 
      java.cjo("getSystemService", name);

    public TelephonyManager telephonyManager => 
      new TelephonyManager(getSystemService(SERVICE_TELEPHONY_MANAGER));

    public Context applicationContext => new Context(java.cjo("getApplicationContext"));

    public PackageManager packageManager => new PackageManager(java.cjo("getPackageManager"));

    public string packageName => java.c<string>("getPackageName");
  }
}
#endif