#if UNITY_ANDROID
using System;
using Assets.Vendor.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
  public static class AndroidActivity {
    public struct DPI {
      public readonly float xDpi, yDpi;

      public DPI(float xDpi, float yDpi) {
        this.xDpi = xDpi;
        this.yDpi = yDpi;
      }

      public override string ToString() {
        return string.Format("DPI[xDpi: {0}, yDpi: {1}]", xDpi, yDpi);
      }
    }

    private static readonly AndroidJavaClass unityPlayer;
    private static readonly AndroidJavaClass bridge;
    public static readonly AndroidJavaObject current;
    public static readonly AndroidJavaObject context;
    public static readonly AndroidJavaObject packageManager;
    public static AndroidJavaObject activity { get { return current; } }
    
    static AndroidActivity() {
      if (Application.isEditor) return;
      unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
      bridge = new AndroidJavaClass("com.tinylabproductions.tlplib.Bridge");
      current = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
      context = current.cjo("getApplicationContext");
      packageManager = context.cjo("getPackageManager");
    }

    /* Get application package name. */
    public static string packageName { get {
      return activity.c<string>("getPackageName");
    } }

    /* Get version code for the application package name. */
    public static int versionCode { get {
      try {
        return packageManager.
          cjo("getPackageInfo", packageName, 0).
          Get<int>("versionCode");
      }
      catch (Exception e) {
        Log.error(e);
        return 0;
      }
    } }

    public static string versionName { get {
      try {
        return packageManager.
          cjo("getPackageInfo", packageName, 0).
          Get<string>("versionName");
      }
      catch (Exception e) {
        Log.error(e);
        return "";
      }
    } }

    public static string rateURL { get {
        return "market://details?id=" + packageName;
    } }

    public static bool isTablet { get { return bridge.CallStatic<bool>("isTablet"); } }

    public static void sharePNG(string path, string title, string sharerText) {
      bridge.CallStatic("sharePNG", path, title, sharerText);
    }

    public static void runOnUI(Act act) { current.Call("runOnUiThread", new AndroidJavaRunnable(act)); }

    public static A runOnUIBlocking<A>(Fn<A> f) {
      return new SyncOtherThreadOp<A>(AndroidUIThreadExecutor.a(f)).execute();
    }

    public static void runOnUIBlocking(Act act) {
      new SyncOtherThreadOp<Unit>(AndroidUIThreadExecutor.a(() => { act(); return new Unit(); })).execute();
    }
  }

  public static class AndroidUIThreadExecutor {
    public static AndroidUIThreadExecutor<A> a<A>(Fn<A> code) { return new AndroidUIThreadExecutor<A>(code); }
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
        try {
          onSuccess(code());
        }
        catch (Exception e) {
          onError(e);
        }
      });
    }
  }
}
#endif