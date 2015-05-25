#if UNITY_ANDROID
using System;
using System.Threading;
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
    public static AndroidJavaObject activity { get { return current; } }
    
    static AndroidActivity() {
      if (Application.isEditor) return;
      unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
      bridge = new AndroidJavaClass("com.tinylabproductions.tlplib.Bridge");
      current = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    /* Get application package name. */
    public static string packageName { get {
      return activity.c<string>("getPackageName");
    } }

    /* Get version code for the application package name. */
    public static string versionCode { get {
      try {
        return activity.cjo("getPackageManager").
          cjo("getPackageInfo", packageName, 0).
          Get<int>("versionCode").ToString();
      }
      catch (Exception e) {
        Log.error(e);
        return "";
      }
    } }

    public static bool isTablet { get { return bridge.CallStatic<bool>("isTablet"); } }

    public static void sharePNG(string path, string title, string sharerText) {
      bridge.CallStatic("sharePNG", path, title, sharerText);
    }

    public static void runOnUI(Act act) { current.Call("runOnUiThread", act); }

    public static A runOnUIBlocking<A>(Func<A> f) {
      return new UIThreadOperation<A>(f).execute();
    }

    public static void runOnUIBlocking(Act act) {
      new UIThreadOperation<Unit>(() => { act(); return new Unit(); }).execute();
    }

  }

  public class UIThreadOperation<A> {
    readonly AutoResetEvent evt = new AutoResetEvent(false);
    readonly int timeoutMs;
    readonly Func<A> func;

    Exception completedException;
    A result;

    public UIThreadOperation(Func<A> func, int timeoutMs = 1000) {
      this.func = func;
      this.timeoutMs = timeoutMs;
    }

    public A execute() {
      AndroidActivity.runOnUI(onUI);
      evt.WaitOne(timeoutMs);
      if (completedException != null) throw completedException;
      return result;
    }

    void onUI() {
      try {
        result = func();
      }
      catch (Exception e) {
        completedException = e;
      }
      finally {
        evt.Set();
      }
    }
  }
}
#endif