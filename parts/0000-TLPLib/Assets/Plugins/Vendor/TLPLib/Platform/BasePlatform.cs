using UnityEngine;

namespace Smooth.Platform {

	/// <summary>
	/// Enumeration representing the base platforms for Unity builds.
	/// </summary>
	public enum BasePlatform {
		None = 0,
		Android = 100,
		BlackBerry = 200,
		Flash = 300,
		Ios = 400,
		Linux = 500,
		Metro = 600,
		NaCl = 700,
		Osx = 800,
#if !UNITY_5_5_OR_NEWER
    Ps3 = 900,
#endif
    Tizen = 1000,
		Windows = 1200,
#if !UNITY_5_3_OR_NEWER
    Wp8 = 1300,
#endif
#if !UNITY_5_5_OR_NEWER
    Xbox360 = 1400,
#endif
	}

	/// <summary>
	/// Extension methods related to the runtime / base platform.
	/// </summary>
	public static class PlatformExtensions {

		/// <summary>
		/// Returns the base platform for the specified runtime platform.
		/// </summary>
		public static BasePlatform ToBasePlatform(this RuntimePlatform runtimePlatform) {
			switch (runtimePlatform) {
			case RuntimePlatform.IPhonePlayer:
				return BasePlatform.Ios;
			case RuntimePlatform.Android:
				return BasePlatform.Android;
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
#if !UNITY_5_4_OR_NEWER
      case RuntimePlatform.WindowsWebPlayer:
#endif
				return BasePlatform.Windows;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
#if !UNITY_5_4_OR_NEWER
			case RuntimePlatform.OSXWebPlayer:
#endif
#if !UNITY_2017_3_OR_NEWER
			case RuntimePlatform.OSXDashboardPlayer:
#endif
				return BasePlatform.Osx;
			case RuntimePlatform.LinuxPlayer:
				return BasePlatform.Linux;
#if UNITY_3_5
			case RuntimePlatform.FlashPlayer:
				return BasePlatform.Flash;
			case RuntimePlatform.NaCl:
				return BasePlatform.NaCl;
#endif
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1
#if UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			case RuntimePlatform.MetroPlayerX86:
			case RuntimePlatform.MetroPlayerX64:
			case RuntimePlatform.MetroPlayerARM:
				return BasePlatform.Metro;
#else
      case RuntimePlatform.WSAPlayerX86:
      case RuntimePlatform.WSAPlayerX64:
      case RuntimePlatform.WSAPlayerARM:
        return BasePlatform.Metro;
#endif
#if !UNITY_5_3_OR_NEWER
			case RuntimePlatform.WP8Player: return BasePlatform.Wp8;
#endif
#if !UNITY_5_4_OR_NEWER
      case RuntimePlatform.BlackBerryPlayer: return BasePlatform.BlackBerry;
#endif
#endif
#if !UNITY_5_5_OR_NEWER
			case RuntimePlatform.XBOX360: return BasePlatform.Xbox360;
			case RuntimePlatform.PS3: return BasePlatform.Ps3;
#endif
#if !UNITY_2017_3_OR_NEWER
      case RuntimePlatform.TizenPlayer: return BasePlatform.Tizen;
#endif
        default:
				return BasePlatform.None;
			}
		}

	  /// <summary>
	  /// Returns true if the specified platform supports JIT compilation; otherwise, false.
	  /// </summary>
	  public static bool HasJit(this RuntimePlatform runtimePlatform) =>
	    runtimePlatform != RuntimePlatform.IPhonePlayer
#if !UNITY_5_5_OR_NEWER
      && runtimePlatform != RuntimePlatform.PS3
      && runtimePlatform != RuntimePlatform.XBOX360
#endif
	    ;

		/// <summary>
		/// Returns true if the specified platform supports JIT compilation; otherwise, false.
		/// </summary>
		public static bool HasJit(this BasePlatform basePlatform) =>
			basePlatform != BasePlatform.Ios
#if !UNITY_5_5_OR_NEWER
      && basePlatform != BasePlatform.Ps3
      && basePlatform != BasePlatform.Xbox360
#endif
        ;
	}
}
