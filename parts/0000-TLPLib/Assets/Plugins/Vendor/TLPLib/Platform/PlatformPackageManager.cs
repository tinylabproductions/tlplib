#if UNITY_ANDROID
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Android;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm;

#endif

namespace com.tinylabproductions.TLPLib.Platform {
  public interface IPlatformPackageManager {
    bool hasAppInstalled(string bundleIdentifier);
    bool hasSystemF
  }

  public static class PlatformPackageManager {
    /// <summary>
    /// Checks for active platform, and returns platform specific package manager
    /// </summary>
    /// <returns>Platform specific package manager</returns>
    public static IPlatformPackageManager a() {
#if UNITY_EDITOR
      return new NoOpPlatformPackageManager();
#elif UNITY_ANDROID
      return new AndroidPlatformPackageManager();
#else
      return new NoOpPlatformPackageManager();
#endif
    }
  }

  class NoOpPlatformPackageManager : IPlatformPackageManager {
    public bool hasAppInstalled(string bundleIdentifier) => false;
  }

#if UNITY_ANDROID
  class AndroidPlatformPackageManager : IPlatformPackageManager {
    readonly PackageManager androidPackageManager;
    readonly ImmutableList<string> packageNames;

    public AndroidPlatformPackageManager() {
      androidPackageManager = AndroidActivity.packageManager;
      //Cache all of the packet names for better performance
      packageNames = androidPackageManager
        .getInstalledPackages(PackageManager.GetPackageInfoFlags.GET_ACTIVITIES)
        .Select(package => package.packageName)
        .ToImmutableList();
    }

    public bool hasSystemFeature(string feature) => androidPackageManager.hasSystemFeature(feature);
    public bool hasAppInstalled(string bundleIdentifier) => packageNames.Contains(bundleIdentifier);
  }
#endif
}