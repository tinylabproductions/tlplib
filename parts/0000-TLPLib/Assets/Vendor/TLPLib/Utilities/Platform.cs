using com.tinylabproductions.TLPLib.Android;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class Platform {
    public const string ANDROID = "android";
    public const string IOS = "ios";
    public const string WP8 = "wp8";
    public const string METRO = "metro";
    public const string BLACKBERRY = "blackberry";
    public const string WEB = "web";
    public const string PC = "pc";
    public const string OTHER = "other";

    public const string SUBNAME_AMAZON = "amazon";
    public const string SUBNAME_OUYA = "ouya";
    public const string SUBNAME_GAMESTICK = "gamestick";
    public const string SUBNAME_OPERA = "opera";
    public const string SUBNAME_TV = "tv";
    public const string SUBNAME_WINDOWS = "windows";
    public const string SUBNAME_OSX = "osx";
    public const string SUBNAME_OSX_DASHBOARD = "osx-dashboard";
    public const string SUBNAME_LINUX = "linux";
    public const string SUBNAME_NONE = "";

    public static string fullName { get {
      var sub = subname;
      return sub == "" ? name : name + "-" + subname;
    } }

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
        case RuntimePlatform.BlackBerryPlayer:
          return BLACKBERRY;
        case RuntimePlatform.WindowsWebPlayer:
        case RuntimePlatform.OSXWebPlayer:
          return WEB;
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
#endif
        if (!Droid.hasSystemFeature("android.hardware.touchscreen")) {
          return SUBNAME_TV;
        }
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
      if (name == WEB) {
        switch (Application.platform) {
          case RuntimePlatform.WindowsWebPlayer:
            return SUBNAME_WINDOWS;
          case RuntimePlatform.OSXWebPlayer:
            return SUBNAME_OSX;
        }
      }

      return SUBNAME_NONE;
    } }
  }
}
