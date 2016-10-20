using com.tinylabproductions.TLPLib.Android;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class Platform {
    public const string
      ANDROID = "android",
      IOS = "ios",
      WP8 = "wp8",
      METRO = "metro",
      BLACKBERRY = "blackberry",
      WEB = "web",
      PC = "pc",
      OTHER = "other",

      SUBNAME_AMAZON = "amazon",
      SUBNAME_OUYA = "ouya",
      SUBNAME_GAMESTICK = "gamestick",
      SUBNAME_OPERA = "opera",
      SUBNAME_TV = "tv",
      SUBNAME_WINDOWS = "windows",
      SUBNAME_OSX = "osx",
      SUBNAME_OSX_DASHBOARD = "osx-dashboard",
      SUBNAME_LINUX = "linux",
      SUBNAME_WILDTANGENT = "wildtangent",
      SUBNAME_NONE = "";

    public static string fullName => subname.nonEmptyOpt().fold(name, s => $"{name}-{s}");

    public static string name { get {
      switch (Application.platform) {
        case RuntimePlatform.Android: 
          return ANDROID;
        case RuntimePlatform.IPhonePlayer: 
          return IOS;
        case RuntimePlatform.WP8Player: 
          return WP8;
#if UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			  case RuntimePlatform.MetroPlayerX86:
			  case RuntimePlatform.MetroPlayerX64:
			  case RuntimePlatform.MetroPlayerARM:
				  return METRO;
#else
        case RuntimePlatform.WSAPlayerX86:
        case RuntimePlatform.WSAPlayerX64:
        case RuntimePlatform.WSAPlayerARM:
          return METRO;
#endif
#if !UNITY_5_4_OR_NEWER
        case RuntimePlatform.BlackBerryPlayer:
          return BLACKBERRY;
        case RuntimePlatform.WindowsWebPlayer:
        case RuntimePlatform.OSXWebPlayer:
          return WEB;
#endif
        case RuntimePlatform.WindowsPlayer:
        case RuntimePlatform.OSXPlayer:
        case RuntimePlatform.OSXDashboardPlayer:
        case RuntimePlatform.LinuxPlayer:
          return PC;
        default: 
          return OTHER;
      }
    } }

    public static string subname { get {
#if UNITY_ANDROID
      if (name == ANDROID) {
#if UNITY_AMAZON
        return SUBNAME_AMAZON;
#elif UNITY_OUYA
        return SUBNAME_OUYA;
#elif UNITY_GAMESTICK
        return SUBNAME_GAMESTICK;
#elif UNITY_OPERA
        return SUBNAME_OPERA;
#elif UNITY_WILDTANGENT
        return SUBNAME_WILDTANGENT;
#endif
        if (!Droid.hasSystemFeature("android.hardware.touchscreen")) return SUBNAME_TV;
      }
#endif
      if (name == PC) {
        switch (Application.platform) {
          case RuntimePlatform.WindowsPlayer:
            return SUBNAME_WINDOWS;
          case RuntimePlatform.OSXPlayer:
            return SUBNAME_OSX;
          case RuntimePlatform.OSXDashboardPlayer:
            return SUBNAME_OSX_DASHBOARD;
          case RuntimePlatform.LinuxPlayer:
            return SUBNAME_LINUX;
        }
      }
#if !UNITY_5_4_OR_NEWER
      if (name == WEB) {
        switch (Application.platform) {
          case RuntimePlatform.WindowsWebPlayer:
            return SUBNAME_WINDOWS;
          case RuntimePlatform.OSXWebPlayer:
            return SUBNAME_OSX;
        }
      }
#endif
      return SUBNAME_NONE;
    } }
  }
}
