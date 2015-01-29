#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
  public static class AndroidActivity {
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

    public static void sharePNG(string path, string title, string sharerText) {
      bridge.CallStatic("sharePNG", path, title, sharerText);
    }

    public static void runOnUI(Act act) { current.Call("runOnUiThread", act); }
  }
}
#endif