#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Android.Bindings.android.app;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm;
using com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.referrer;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
using Application = UnityEngine.Application;

namespace com.tinylabproductions.TLPLib.Android {
  public static class AndroidActivity {
    public struct DPI {
      public readonly float xDpi, yDpi;

      public DPI(float xDpi, float yDpi) {
        this.xDpi = xDpi;
        this.yDpi = yDpi;
      }

      public override string ToString() => $"DPI[xDpi: {xDpi}, yDpi: {yDpi}]";
    }

    public static readonly Activity current;
    public static readonly Context appContext;
    public static readonly PackageManager packageManager;
    /* Application package name. */
    public static readonly string packageName;
    public static Activity activity => current;

    static AndroidActivity() {
      if (Application.isEditor) return;

      using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
        current = new Activity(unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"));
        appContext = current.applicationContext;
        packageManager = appContext.packageManager;
        packageName = activity.packageName;
      }
    }

    /* Get version code for the application package name. */
    public static int versionCode { get {
      try {
        return packageManager.java.
          cjo("getPackageInfo", packageName, 0).
          Get<int>("versionCode");
      }
      catch (Exception e) {
        Log.d.error(e);
        return 0;
      }
    } }

    public static string versionName { get {
      try {
        return packageManager.java.
          cjo("getPackageInfo", packageName, 0).
          Get<string>("versionName");
      }
      catch (Exception e) {
        Log.d.error(e);
        return "";
      }
    } }

    public static string rateURL => "market://details?id=" + packageName;

    public static void runOnUI(Action act) => current.runOnUIThread(act);
    public static Future<A> runOnUI<A>(Fn<A> f) => Future<A>.async(promise => runOnUI(() => {
      var ret = f();
      ASync.OnMainThread(() => promise.complete(ret));
    }));

    public static A runOnUIBlocking<A>(Fn<A> f) =>
      SyncOtherThreadOp.a(AndroidUIThreadExecutor.a(f)).execute();

    public static void runOnUIBlocking(Action act) =>
      runOnUIBlocking(() => { act(); return new Unit(); });

    /**
     * To use this, add the following to your AndroidManifest.xml
     *
     * <receiver
     *   android:name="com.tinylabproductions.tlplib.referrer.InstallReferrerReceiver"
     *   android:exported="true"
     * >
     *   <intent-filter>
     *     <action android:name="com.android.vending.INSTALL_REFERRER" />
     *   </intent-filter>
     * </receiver>
     */
    public static Option<string> installReferrer =>
      InstallReferrerReceiver.preferences(current)
      .getString(InstallReferrerReceiver.PREF_REFERRER);
  }

  public static class AndroidUIThreadExecutor {
    public static AndroidUIThreadExecutor<A> a<A>(Fn<A> code) => new AndroidUIThreadExecutor<A>(code);
  }

  // This takes 10 ms on galaxy S5
  // 1 ms on Acer A110
  public class AndroidUIThreadExecutor<A> : OtherThreadExecutor<A> {
    readonly Fn<A> code;

    public AndroidUIThreadExecutor(Fn<A> code) {
      this.code = code;
    }

    public void execute(Act<A> onSuccess, Act<Exception> onError) {
      AndroidActivity.runOnUI(() => {
        try { onSuccess(code()); }
        catch (Exception e) { onError(e); }
      });
    }
  }
}
#endif