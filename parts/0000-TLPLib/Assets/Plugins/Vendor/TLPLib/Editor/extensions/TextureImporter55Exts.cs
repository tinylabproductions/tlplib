#if UNITY_5_5_OR_NEWER
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  public static class TextureImporterExts {
    public class Platform {
      //https://docs.unity3d.com/ScriptReference/TextureImporterPlatformSettings-name.html
      public static Platform 
        android = a("Android"),
        standalone = a("Standalone"),
        web = a("Web"),
        iPhone = a("iPhone"),
        webGL = a("WebGL"),
        windowsStoreApps = a("Windows Store Apps"),
        tizen = a("Tizen"),
        PSP2 = a("PSP2"),
        PS4 = a("PS4"),
        PSM = a("PSM"),
        XboxOne = a("XboxOne"),
        samsungTV = a("Samsung TV"),
        nintendo3DS = a("Nintendo 3DS"),
        wiiU = a("WiiU"),
        tvOS = a("tvOS"),
        defaultPlatform = a("DefaultTexturePlatform");

      public readonly string name;

      Platform(string name) { this.name = name; }

      static Platform a(string name) => new Platform(name);
    }

    public static TextureImporterPlatformSettings getPlatformSettings(
      this TextureImporter ti, Platform platform
    ) => ti.GetPlatformTextureSettings(platform.name);

    public static TextureImporterPlatformSettings setPlatform(
      this TextureImporterPlatformSettings ps, Platform platform
    ) {
      ps.name = platform.name;
      return ps;
    }

    public static string toString(this TextureImporterPlatformSettings ps) =>
      $"{nameof(ps.textureCompression)}:{ps.textureCompression}, " +
      $"{nameof(ps.allowsAlphaSplitting)}:{ps.allowsAlphaSplitting}, " +
      $"{nameof(ps.compressionQuality)}:{ps.compressionQuality}, " +
      $"{nameof(ps.crunchedCompression)}:{ps.crunchedCompression}, " +
      $"{nameof(ps.format)}:{ps.format}, " +
      $"{nameof(ps.maxTextureSize)}:{ps.maxTextureSize}, " +
      $"{nameof(ps.name)}:{ps.name}, " +
      $"{nameof(ps.overridden)}:{ps.overridden}";
  }
  }
}
#endif