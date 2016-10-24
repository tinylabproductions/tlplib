#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.google.android.gms.ads.identifier {
  public class AdvertisingIdClient {
    /** Includes both the advertising ID as well as the limit ad tracking setting. */
    public class Info : Binding {
      public Info(AndroidJavaObject java) : base(java) {}

      public string id => java.Call<string>("getId");
      public bool limitAdTrackingEnabled => java.c<bool>("isLimitAdTrackingEnabled");
    }

    static readonly AndroidJavaClass klass = 
      new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");

    public static Try<Info> getAdvertisingIdInfo(Context context) =>
      F.doTry(() => new Info(klass.csjo("getAdvertisingIdInfo", context.java)));
  }
}
#endif