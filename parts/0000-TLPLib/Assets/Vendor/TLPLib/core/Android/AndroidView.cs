#if UNITY_ANDROID

using System;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.concurrent;
using pzd.lib.log;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
  public static class AndroidView {
    const string
      FLAG_HIDE_NAVIGATION = "SYSTEM_UI_FLAG_HIDE_NAVIGATION",
      FLAG_STABLE_LAYOUT = "SYSTEM_UI_FLAG_LAYOUT_STABLE",
      FLAG_IMMERSIVE_STICKY = "SYSTEM_UI_FLAG_IMMERSIVE_STICKY",
      FLAG_FULLSCREEN = "SYSTEM_UI_FLAG_FULLSCREEN";

    static readonly AndroidJavaClass view;

    static AndroidView() {
      if (Application.isEditor) return;
      view = new AndroidJavaClass("android.view.View");
    }

    /* [2017-05-18] usage of this method was removed.
     * With Unity 5.5 screen resizing randomly while loading issue came up
     * and removing this seems like it fixes the issue */
    /* If the layout is stable, screen space never changes, but navigation buttons just
     * become a black stripe and never hides. */
    public static Future<bool> hideNavigationBar(bool stableLayout) {
      if (Application.platform != RuntimePlatform.Android) return Future.successful(false);

      if (Log.d.isDebug()) Log.d.debug("Trying to hide android navigation bar.");
      var activity = AndroidActivity.current;
      return Future.async<bool>(p => {
        AndroidActivity.runOnUI(() => {
          try {
            var flags =
              view.GetStatic<int>(FLAG_HIDE_NAVIGATION) |
              view.GetStatic<int>(FLAG_IMMERSIVE_STICKY) |
              view.GetStatic<int>(FLAG_FULLSCREEN);
            if (stableLayout) flags |= view.GetStatic<int>(FLAG_STABLE_LAYOUT);
            var decor = activity.java.
              Call<AndroidJavaObject>("getWindow").
              Call<AndroidJavaObject>("getDecorView");
            decor.Call("setSystemUiVisibility", flags);
            if (Log.d.isDebug()) Log.d.debug("Hiding android navigation bar succeeded.");
            p.complete(true);
          }
          catch (Exception e) {
            if (Log.d.isDebug()) Log.d.debug(
              "Error while trying to hide navigation bar on android: " + e
            );
            p.complete(false);
          }
        });
      });
    }
  }
}
#endif
