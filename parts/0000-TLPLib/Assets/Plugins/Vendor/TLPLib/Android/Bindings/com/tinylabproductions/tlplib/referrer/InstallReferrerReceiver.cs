#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.referrer {
  public class InstallReferrerReceiver {
    static readonly AndroidJavaClass klass;
    public static readonly string PREF_REFERRER;

    static InstallReferrerReceiver() {
      klass = new AndroidJavaClass(
        "com.tinylabproductions.tlplib.referrer.InstallReferrerReceiver"
      );
      PREF_REFERRER = klass.GetStatic<string>("PREF_REFERRER");
    }

    public static SharedPreferences preferences(Context ctx) => 
      new SharedPreferences(klass.csjo("getPrefs", ctx.java));
  }
}
#endif