#if UNITY_EDITOR
using System;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.Extensions {
  public static class BuildTargetExts {
    public static BuildTargetGroup toGroup(this BuildTarget t) {
      switch (t) {
        case BuildTarget.StandaloneOSXUniversal: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneOSXIntel: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneWindows: return BuildTargetGroup.Standalone;
        case BuildTarget.WebPlayer: return BuildTargetGroup.WebPlayer;
        case BuildTarget.WebPlayerStreamed: return BuildTargetGroup.WebPlayer;
        case BuildTarget.iOS: return BuildTargetGroup.iOS;
        case BuildTarget.PS3: return BuildTargetGroup.PS3;
        case BuildTarget.XBOX360: return BuildTargetGroup.XBOX360;
        case BuildTarget.Android: return BuildTargetGroup.Android;
        case BuildTarget.StandaloneGLESEmu: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneLinux: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneWindows64: return BuildTargetGroup.Standalone;
        case BuildTarget.WebGL: return BuildTargetGroup.WebGL;
        case BuildTarget.WSAPlayer: return BuildTargetGroup.WSA;
        case BuildTarget.StandaloneLinux64: return BuildTargetGroup.Standalone;
        case BuildTarget.StandaloneLinuxUniversal: return BuildTargetGroup.Standalone;
        case BuildTarget.WP8Player: return BuildTargetGroup.WP8;
        case BuildTarget.StandaloneOSXIntel64: return BuildTargetGroup.Standalone;
        case BuildTarget.BlackBerry: return BuildTargetGroup.BlackBerry;
        case BuildTarget.Tizen: return BuildTargetGroup.Tizen;
        case BuildTarget.PSP2: return BuildTargetGroup.PSP2;
        case BuildTarget.PS4: return BuildTargetGroup.PS4;
        case BuildTarget.PSM: return BuildTargetGroup.PSM;
        case BuildTarget.XboxOne: return BuildTargetGroup.XboxOne;
        case BuildTarget.SamsungTV: return BuildTargetGroup.SamsungTV;
        case BuildTarget.Nintendo3DS: return BuildTargetGroup.Nintendo3DS;
        case BuildTarget.WiiU: return BuildTargetGroup.WiiU;
        case BuildTarget.tvOS: return BuildTargetGroup.tvOS;
        default:
          throw new ArgumentOutOfRangeException(
            nameof(t), t, $"Are you using obsolete {nameof(BuildTarget)}?"
          );
      }
    }
  }
}
#endif