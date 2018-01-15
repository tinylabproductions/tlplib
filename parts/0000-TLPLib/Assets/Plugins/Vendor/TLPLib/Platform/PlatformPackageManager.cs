using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Platform {
  public interface IPlatformPackageManager {
    bool hasAppInstalled(string bundleIdentifier);
    Option<ErrorMsg> openApp(string bundleIdentifier);
  }

  public static class PlatformPackageManager {
    public static readonly IPlatformPackageManager packageManager =
#if UNITY_ANDROID
      Application.isEditor 
        ? (IPlatformPackageManager) new NoOpPlatformPackageManager() 
        : new AndroidPlatformPackageManager();
#else
      new NoOpPlatformPackageManager();
#endif
  }

  class NoOpPlatformPackageManager : IPlatformPackageManager {
    public bool hasAppInstalled(string bundleIdentifier) => false;
    public Option<ErrorMsg> openApp(string bundleIdentifier) => Option<ErrorMsg>.None;
  }
}