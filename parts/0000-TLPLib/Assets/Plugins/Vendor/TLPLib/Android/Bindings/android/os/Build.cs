#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.os {
  public static class Build {
    public static class Version {
      static readonly AndroidJavaClass klass = new AndroidJavaClass("android.os.Build$VERSION");

      public static readonly string SDK = klass.GetStatic<string>("SDK");
      public static readonly int SDK_INT = klass.GetStatic<int>("SDK_INT");

      public const int MARSHMALLOW_SDK_INT = 23;
    }
  }
}
#endif