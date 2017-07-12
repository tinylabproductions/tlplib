#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm {
  public class PackageManager : Binding {
    public enum GetPackageInfoFlags {
      GET_ACTIVITIES = 1,
      GET_CONFIGURATIONS = 16384,
      GET_GIDS = 256,
      GET_INSTRUMENTATION = 16,
      GET_INTENT_FILTERS = 32,
      GET_META_DATA = 128,
      GET_PERMISSIONS = 4096,
      GET_PROVIDERS = 8,
      GET_RECEIVERS = 2,
      GET_SERVICES = 4,
      GET_SHARED_LIBRARY_FILES = 1024,
      GET_SIGNATURES = 64,
      GET_URI_PERMISSION_PATTERNS = 2048,
      MATCH_DISABLED_COMPONENTS = 512,
      MATCH_DISABLED_UNTIL_USED_COMPONENTS = 32768,
      MATCH_UNINSTALLED_PACKAGES = 8192
    }

    public PackageManager(AndroidJavaObject java) : base(java) {}

    public bool hasSystemFeature(string feature) => 
      Application.platform != RuntimePlatform.Android || java.Call<bool>("hasSystemFeature", feature);

    // https://developer.android.com/reference/android/content/pm/PackageManager.html#getPackageInfo(java.lang.String,%20int)
    public Option<PackageInfo> getPackageInfo(string bundleIdentifier, GetPackageInfoFlags flags) {
      if (Application.platform != RuntimePlatform.Android) return Option<PackageInfo>.None;
      try {
        return F.some(new PackageInfo(
          java.cjo("getPackageInfo", bundleIdentifier, (int) flags)
        ));
      }
      catch (AndroidJavaException) {
        return Option<PackageInfo>.None;
      }
    }
  }
}
#endif
