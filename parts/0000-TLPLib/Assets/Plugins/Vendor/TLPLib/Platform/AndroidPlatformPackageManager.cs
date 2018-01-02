#if UNITY_ANDROID
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Android;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Platform {
  class AndroidPlatformPackageManager : IPlatformPackageManager {
    readonly ImmutableSortedSet<string> packageNames;

    public AndroidPlatformPackageManager() {
      //Cache all of the package names for better performance
      packageNames =
        AndroidActivity.packageManager
          .getInstalledPackages(PackageManager.GetPackageInfoFlags.GET_ACTIVITIES)
          .Select(package => package.packageName)
          .ToImmutableSortedSet();
    }

    public bool hasAppInstalled(string bundleIdentifier) => packageNames.Contains(bundleIdentifier);
    public Try<Unit> openApp(string bundleIdentifier) => 
      AndroidActivity.packageManager.openApp(AndroidActivity.current, bundleIdentifier);
  }
}
#endif