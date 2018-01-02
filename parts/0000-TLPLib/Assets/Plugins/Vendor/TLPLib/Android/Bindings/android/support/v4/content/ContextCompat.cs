#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.support.v4.content {
  public class ContextCompat {
    static readonly AndroidJavaClass klass = 
      new AndroidJavaClass("android.support.v4.content.ContextCompat");

    public static bool checkSelfPermission(
      string permission, Context context = null
    ) => klass.CallStatic<int>(
      "checkSelfPermission", (context ?? AndroidActivity.activity).java, permission
    ) == 0;
  }
}
#endif