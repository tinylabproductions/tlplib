#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.os {
  public static class Build {
    public static class Version {
      static readonly AndroidJavaClass klass = new AndroidJavaClass("android.os.Build$VERSION");

      public static readonly string SDK = klass.GetStatic<string>("SDK");
      public static readonly int SDK_INT = klass.GetStatic<int>("SDK_INT");
    }

    static readonly AndroidJavaClass klass = new AndroidJavaClass("android.os.Build");
    public static readonly string MANUFACTURER = klass.GetStatic<string>("MANUFACTURER");
    public static readonly string DEVICE = klass.GetStatic<string>("DEVICE");
  }
}
#endif