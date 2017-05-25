#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm {
  public class PackageInfo : Binding {
    public readonly string packageName;

    public PackageInfo(AndroidJavaObject java) : base(java) {
      // Cache the field.
      packageName = java.Get<string>("packageName");
    }
  }
}
#endif