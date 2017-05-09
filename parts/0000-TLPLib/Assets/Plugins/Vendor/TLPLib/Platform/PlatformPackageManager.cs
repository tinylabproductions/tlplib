#if UNITY_ANDROID
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Android;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm;
#endif

namespace com.tinylabproductions.TLPLib.Platform {
  public interface IPlatformPackageManager {
    bool hasAppInstalled(string bundleIdentifier);
  }

  public static class PlatformPackageManager {
    public static readonly IPlatformPackageManager packageManager =
#if UNITY_EDITOR
      new NoOpPlatformPackageManager();
#elif UNITY_ANDROID
      new AndroidPlatformPackageManager();
#else
      new NoOpPlatformPackageManager();
#endif
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
      packageNames = 
        androidPackageManager
          .getInstalledPackages(PackageManager.GetPackageInfoFlags.GET_ACTIVITIES)
          .Select(package => package.packageName)
          .ToImmutableList();
    }

    public bool hasAppInstalled(string bundleIdentifier) => packageNames.Contains(bundleIdentifier);
  }
#endif
}