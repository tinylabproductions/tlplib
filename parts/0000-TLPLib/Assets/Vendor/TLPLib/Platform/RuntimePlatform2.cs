using System.Collections.ObjectModel;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Platform {
  public static class RuntimePlatform2 {
    public static readonly ReadOnlyCollection<RuntimePlatform> all = new ReadOnlyCollection<RuntimePlatform>(
      new[] {
        RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer,
        RuntimePlatform.OSXEditor, RuntimePlatform.OSXPlayer,
        RuntimePlatform.LinuxPlayer,
        RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.OSXWebPlayer,
        RuntimePlatform.OSXDashboardPlayer, RuntimePlatform.WindowsWebPlayer, RuntimePlatform.PS3,
        RuntimePlatform.XBOX360,
        RuntimePlatform.Android,
        RuntimePlatform.LinuxPlayer,
        RuntimePlatform.WebGLPlayer,
        RuntimePlatform.WSAPlayerX86,
        RuntimePlatform.WSAPlayerX64,
        RuntimePlatform.WSAPlayerARM,
        RuntimePlatform.WP8Player,
        RuntimePlatform.BlackBerryPlayer,
        RuntimePlatform.TizenPlayer,
        RuntimePlatform.PSP2,
        RuntimePlatform.PS4,
        RuntimePlatform.PSM,
        RuntimePlatform.XboxOne,
        RuntimePlatform.SamsungTVPlayer,
        RuntimePlatform.WiiU,
        RuntimePlatform.tvOS,
      }
    );
  }
}
