#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm {
  public class PackageInfo : Binding {
    public readonly string packageName;

    public PackageInfo(AndroidJavaObject java) : base(java) {
      // Cache the field.
      packageName = java.Get<string>("packageName");
    }

    // https://developer.android.com/reference/android/content/pm/PackageInfo.html#requestedPermissions
    public string[] requestedPermissions =>
      java.Get<string[]>("requestedPermissions").orElseIfNull(F.emptyArray<string>());
  }
}
#endif