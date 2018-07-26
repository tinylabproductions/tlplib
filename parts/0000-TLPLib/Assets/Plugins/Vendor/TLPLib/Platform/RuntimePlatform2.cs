using System.Collections.ObjectModel;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Platform {
  public static class RuntimePlatform2 {
    public static readonly ReadOnlyCollection<RuntimePlatform> all = new ReadOnlyCollection<RuntimePlatform>(
      new[] {
        RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer,
        RuntimePlatform.OSXEditor, RuntimePlatform.OSXPlayer,
        RuntimePlatform.LinuxPlayer,
        RuntimePlatform.Android, RuntimePlatform.IPhonePlayer,
        RuntimePlatform.Android,
        RuntimePlatform.LinuxPlayer,
        RuntimePlatform.WebGLPlayer,
        RuntimePlatform.WSAPlayerX86,
        RuntimePlatform.WSAPlayerX64,
        RuntimePlatform.WSAPlayerARM,
        RuntimePlatform.PSP2,
        RuntimePlatform.PS4,
        RuntimePlatform.XboxOne,
        RuntimePlatform.tvOS,
#if !UNITY_5_3_OR_NEWER
        RuntimePlatform.WP8Player,
        RuntimePlatform.PSM,
#endif
#if !UNITY_5_4_OR_NEWER
        RuntimePlatform.OSXWebPlayer,
        RuntimePlatform.BlackBerryPlayer,
        RuntimePlatform.WindowsWebPlayer,
#endif
#if !UNITY_5_5_OR_NEWER
        RuntimePlatform.PS3,
        RuntimePlatform.XBOX360,
#endif
#if !UNITY_2017_3_OR_NEWER
        RuntimePlatform.OSXDashboardPlayer,
        RuntimePlatform.TizenPlayer,
        RuntimePlatform.SamsungTVPlayer,
#endif
#if !UNITY_2018_1_OR_NEWER
        RuntimePlatform.WiiU,
#endif
      }
    );
  }
}
