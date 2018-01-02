using System;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Platform {
  public interface IPlatformPackageManager {
    bool hasAppInstalled(string bundleIdentifier);
    Try<Unit> openApp(string bundleIdentifier);
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
    public Try<Unit> openApp(string bundleIdentifier) => F.scs(F.unit);
  }
}