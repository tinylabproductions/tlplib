#if UNITY_ANDROID
using pzd.lib.functional;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Android.Bindings.java.util;
using com.tinylabproductions.TLPLib.Data;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using Application = UnityEngine.Application;

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
    public Try<PackageInfo> getPackageInfo(
      string bundleIdentifier, GetPackageInfoFlags flags
    ) => F.doTry(
      () => new PackageInfo(java.cjo("getPackageInfo", bundleIdentifier, (int) flags))
    );

    // https://developer.android.com/reference/android/content/pm/PackageManager.html#getPermissionInfo(java.lang.String,%20int)
    public Option<PermissionInfo> getPermissionInfo(
      string permission, bool getMetaData = false
    ) {
      var flags = getMetaData ? (int) GetPackageInfoFlags.GET_META_DATA : 0;
      try {
        return new PermissionInfo(
          permission, java.cjo("getPermissionInfo", permission, flags)
        ).some();
      }
      catch (AndroidJavaException e) {
        if (e.Message.StartsWithFast("android.content.pm.PackageManager$NameNotFoundException:"))
          return None._;
        else
          throw;
      }
    }

    // https://developer.android.com/reference/android/content/pm/PackageManager.html#getInstalledPackages(int)
    public ImmutableList<PackageInfo> getInstalledPackages(GetPackageInfoFlags flags) {
      // List<PackageInfo> getInstalledPackages(int flags)
      var jList = new List(java.cjo("getInstalledPackages", (int) flags));
      return jList.Select(jo => new PackageInfo(jo)).ToImmutableList();
    }

    // https://developer.android.com/reference/android/content/pm/PackageManager.html#getLaunchIntentForPackage(java.lang.String)
    public Option<Intent> getLaunchIntentForPackage(string bundleIdentifier) =>
      F.opt(java.cjo("getLaunchIntentForPackage", bundleIdentifier)).map(_ => new Intent(_));

    /// <summary>
    /// Convenience method for launching an app by bundle identifier.
    /// </summary>
    public Option<ErrorMsg> openApp(string bundleIdentifier, Context context = null) =>
      getLaunchIntentForPackage(bundleIdentifier).fold(
        () => Some.a(new ErrorMsg($"Unknown bundle identifier '{bundleIdentifier}'")),
        intent => (context ?? AndroidActivity.current).startActivity(intent).fold(
          None._,
          ex => Some.a(new ErrorMsg($"Error while launching app '{bundleIdentifier}': {ex}"))
        )
      );
  }
}
#endif