#if UNITY_ANDROID
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Android;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Platform {
  class AndroidPlatformPackageManager : IPlatformPackageManager {
    public ImmutableSortedSet<string> packageNames { get; }

    public AndroidPlatformPackageManager() {
      //Cache all of the package names for better performance
      packageNames =
        AndroidActivity.packageManager
          .getInstalledPackages(PackageManager.GetPackageInfoFlags.GET_ACTIVITIES)
          .Select(package => package.packageName)
          .ToImmutableSortedSet();
    }

    public bool hasAppInstalled(string bundleIdentifier) => packageNames.Contains(bundleIdentifier);
    public Option<ErrorMsg> openApp(string bundleIdentifier) =>
      AndroidActivity.packageManager.openApp(bundleIdentifier);
  }
}
#endif