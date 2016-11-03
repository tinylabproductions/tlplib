#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.Extensions {
  public static class BuildTargetExts {
    public static RuntimePlatform toRuntimePlatform(this BuildTarget t) {
      switch (t) {
        case BuildTarget.StandaloneOSXUniversal:
        case BuildTarget.StandaloneOSXIntel:
        case BuildTarget.StandaloneOSXIntel64:
          return RuntimePlatform.OSXPlayer;
        case BuildTarget.StandaloneWindows:
        case BuildTarget.StandaloneWindows64:
          return RuntimePlatform.WindowsPlayer;
        case BuildTarget.StandaloneLinux64:
        case BuildTarget.StandaloneLinuxUniversal:
        case BuildTarget.StandaloneLinux:
          return RuntimePlatform.LinuxPlayer;
        case BuildTarget.iOS: return RuntimePlatform.IPhonePlayer;
        case BuildTarget.PS3: return RuntimePlatform.PS3;
        case BuildTarget.XBOX360: return RuntimePlatform.XBOX360;
        case BuildTarget.Android: return RuntimePlatform.Android;
        case BuildTarget.WebGL: return RuntimePlatform.WebGLPlayer;
        case BuildTarget.Tizen: return RuntimePlatform.TizenPlayer;
        case BuildTarget.PSP2: return RuntimePlatform.PSP2;
        case BuildTarget.PS4: return RuntimePlatform.PS4;
        case BuildTarget.PSM: return RuntimePlatform.PSM;
        case BuildTarget.XboxOne: return RuntimePlatform.XboxOne;
        case BuildTarget.SamsungTV: return RuntimePlatform.SamsungTVPlayer;
        case BuildTarget.WiiU: return RuntimePlatform.WiiU;
        case BuildTarget.tvOS: return RuntimePlatform.tvOS;
#if !UNITY_5_4_OR_NEWER
        case BuildTarget.WP8Player: return RuntimePlatform.WP8Player;
        case BuildTarget.BlackBerry: return RuntimePlatform.BlackBerryPlayer;
        case BuildTarget.StandaloneGLESEmu:
#endif
        case BuildTarget.WebPlayer:
        case BuildTarget.WebPlayerStreamed:
        case BuildTarget.Nintendo3DS:
        case BuildTarget.WSAPlayer:
          throw new ArgumentOutOfRangeException(
            nameof(t), t, $"Can't convert to {nameof(RuntimePlatform)}"
          );

        default:
          throw new ArgumentOutOfRangeException(
            nameof(t), t, $"Are you using obsolete {nameof(BuildTarget)}?"
          );
      }
    }

    public static BuildTargetGroup toGroup(this BuildTarget t) {
      switch (t) {
        case BuildTarget.StandaloneOSXUniversal: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneOSXIntel: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneWindows: return BuildTargetGroup.Standalone;
        case BuildTarget.iOS: return BuildTargetGroup.iOS;
        case BuildTarget.PS3: return BuildTargetGroup.PS3;
        case BuildTarget.XBOX360: return BuildTargetGroup.XBOX360;
        case BuildTarget.Android: return BuildTargetGroup.Android;
        case BuildTarget.StandaloneLinux: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneWindows64: return BuildTargetGroup.Standalone;
        case BuildTarget.WebGL: return BuildTargetGroup.WebGL;
        case BuildTarget.WSAPlayer: return BuildTargetGroup.WSA;
        case BuildTarget.StandaloneLinux64: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneLinuxUniversal: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneOSXIntel64: return BuildTargetGroup.Standalone;
        case BuildTarget.Tizen: return BuildTargetGroup.Tizen;
        case BuildTarget.PSP2: return BuildTargetGroup.PSP2;
        case BuildTarget.PS4: return BuildTargetGroup.PS4;
        case BuildTarget.PSM: return BuildTargetGroup.PSM;
        case BuildTarget.XboxOne: return BuildTargetGroup.XboxOne;
        case BuildTarget.SamsungTV: return BuildTargetGroup.SamsungTV;
        case BuildTarget.Nintendo3DS: return BuildTargetGroup.Nintendo3DS;
        case BuildTarget.WiiU: return BuildTargetGroup.WiiU;
        case BuildTarget.tvOS: return BuildTargetGroup.tvOS;
#if !UNITY_5_4_OR_NEWER
        case BuildTarget.StandaloneGLESEmu: return BuildTargetGroup.Standalone;
        case BuildTarget.WebPlayer: return BuildTargetGroup.WebPlayer;
        case BuildTarget.WebPlayerStreamed: return BuildTargetGroup.WebPlayer;
        case BuildTarget.WP8Player: return BuildTargetGroup.WP8;
        case BuildTarget.BlackBerry: return BuildTargetGroup.BlackBerry;
#endif
        default:
          throw new ArgumentOutOfRangeException(
            nameof(t), t, $"Are you using obsolete {nameof(BuildTarget)}?"
          );
      }
    }
  }
}
#endif