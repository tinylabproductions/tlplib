using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace Code.Utils {
  public static class TextureSize {
    // Intentionally a function, because this value is not used often.
    public static PrefVal<bool> devForcedFastestQualitySetting =>
      PrefVal.player.boolean("forced-fastest-quality-setting", false);

    public static ISubscription registerTextureConsole(IDisposableTracker tracker) =>
      DConsole.instance.registrarOnShow(
        tracker, "Textures",
        (dc, r) => {
          r.register("Texture format support", () =>
            EnumUtils.GetValues<TextureFormat>()
              .Select(tf => $"[{tf}] {tf.isSupported()}")
              .asDebugString()
          );
          r.register("Render texture format support", () =>
            EnumUtils.GetValues<RenderTextureFormat>()
              .Select(tf => $"[{tf}] {SystemInfo.SupportsRenderTextureFormat(tf)}")
              .asDebugString()
          );
          r.register("Max texture size", () => SystemInfo.maxTextureSize);
          r.register("Textures size info", () => {
            var all = Resources.FindObjectsOfTypeAll<Texture2D>()
              .Select(tex => F.t(tex.name, textureSizeInBytes(tex)))
              .ToArray();
            Log.d.debug(all.Where(t => t._2 > 1024).OrderByDescending(t => t._2).Select(t => $"{t._2 / 1024,7:F0} KB [{t._1}]").asDebugString());
            return all.Select(t => t._2).Sum() / 1024 / 1024 + " MB";
          });
          r.register(nameof(SystemInfo.systemMemorySize), () => SystemInfo.systemMemorySize);
          r.register(nameof(SystemInfo.graphicsMemorySize), () => SystemInfo.graphicsMemorySize);
          r.register(nameof(QualitySettings.GetQualityLevel), QualitySettings.GetQualityLevel);
          foreach (var b in DConsole.bools)
            r.register($"{nameof(devForcedFastestQualitySetting)} to {b}", () => devForcedFastestQualitySetting.value = b);
        }
      );

    public static float textureSizeInBytes(Texture2D tex) {
      var size = tex.width * tex.height * bitsPerPixelOnCurrentDevice(tex.format) / 8f;
      var res = 0f;
      for (var i = 0; i < tex.mipmapCount; i++) {
        res += size;
        size /= 4;
      }
      return res;
    }

    public static float bitsPerPixelOnCurrentDevice(TextureFormat tf) {
      if (tf.isSupported()) return bitsPerPixel(tf);
      // not exactly correct in all cases but it should work for what we use
      return containsAlpha(tf) ? 32 : 24;
    }

    public static bool containsAlpha(TextureFormat tf) {
      #pragma warning disable SwitchEnumAnalyzer
      switch (tf) {
        case TextureFormat.Alpha8: return true;
        case TextureFormat.ARGB4444: return true;
        case TextureFormat.RGB24: return false;
        case TextureFormat.RGBA32: return true;
        case TextureFormat.ARGB32: return true;
        case TextureFormat.RGB565: return false;
        case TextureFormat.R16: return false;
        case TextureFormat.DXT1: return false;
        case TextureFormat.DXT5: return true;
        case TextureFormat.RGBA4444: return true;
        case TextureFormat.BGRA32: return true;
        case TextureFormat.RHalf: return false;
        case TextureFormat.RGHalf: return false;
        case TextureFormat.RGBAHalf: return true;
        case TextureFormat.RFloat: return false;
        case TextureFormat.RGFloat: return false;
        case TextureFormat.RGBAFloat: return true;
        case TextureFormat.YUY2: return false;
#if !UNITY_IOS
        case TextureFormat.DXT1Crunched: return false;
        case TextureFormat.DXT5Crunched: return true;
#endif
        case TextureFormat.PVRTC_RGB2: return false;
        case TextureFormat.PVRTC_RGBA2: return true;
        case TextureFormat.PVRTC_RGB4: return false;
        case TextureFormat.PVRTC_RGBA4: return true;
        case TextureFormat.ETC_RGB4: return false;
        case TextureFormat.EAC_R: return false;
        case TextureFormat.EAC_R_SIGNED: return false;
        case TextureFormat.EAC_RG: return false;
        case TextureFormat.EAC_RG_SIGNED: return false;
        case TextureFormat.ETC2_RGB: return false;
        case TextureFormat.ETC2_RGBA1: return true;
        case TextureFormat.ETC2_RGBA8: return true;
        case TextureFormat.ASTC_RGB_4x4: return false;
        case TextureFormat.ASTC_RGB_5x5: return false;
        case TextureFormat.ASTC_RGB_6x6: return false;
        case TextureFormat.ASTC_RGB_8x8: return false;
        case TextureFormat.ASTC_RGB_10x10: return false;
        case TextureFormat.ASTC_RGB_12x12: return false;
        case TextureFormat.ASTC_RGBA_4x4: return true;
        case TextureFormat.ASTC_RGBA_5x5: return true;
        case TextureFormat.ASTC_RGBA_6x6: return true;
        case TextureFormat.ASTC_RGBA_8x8: return true;
        case TextureFormat.ASTC_RGBA_10x10: return true;
        case TextureFormat.ASTC_RGBA_12x12: return true;
        case TextureFormat.ETC_RGB4_3DS: return false;
        case TextureFormat.ETC_RGBA8_3DS: return true;
        case TextureFormat.BC4: return false;
        case TextureFormat.BC5: return false;
        case TextureFormat.BC6H: return false;
        case TextureFormat.BC7: return true;
        case TextureFormat.RGB9e5Float: return false;
        case TextureFormat.RG16: return false;
        case TextureFormat.R8: return false;
#if !UNITY_IOS
        case TextureFormat.ETC_RGB4Crunched: return false;
        case TextureFormat.ETC2_RGBA8Crunched: return true;
#endif
        default:
          throw new ArgumentOutOfRangeException(nameof(tf), tf, null);
      }
      #pragma warning restore SwitchEnumAnalyzer
    }

    public static float bitsPerPixel(TextureFormat tf) {
      #pragma warning disable SwitchEnumAnalyzer
      switch (tf) {
        case TextureFormat.Alpha8: return 8;
        case TextureFormat.ARGB4444: return 16;
        case TextureFormat.RGB24: return 24;
        case TextureFormat.RGBA32: return 32;
        case TextureFormat.ARGB32: return 32;
        case TextureFormat.RGB565: return 16;
        case TextureFormat.R16: return 16;
        case TextureFormat.DXT1: return 4;
        case TextureFormat.DXT5: return 8;
        case TextureFormat.RGBA4444: return 16;
        case TextureFormat.BGRA32: return 32;
        case TextureFormat.RHalf: return 16;
        case TextureFormat.RGHalf: return 32;
        case TextureFormat.RGBAHalf: return 64;
        case TextureFormat.RFloat: return 32;
        case TextureFormat.RGFloat: return 64;
        case TextureFormat.RGBAFloat: return 128;
        case TextureFormat.YUY2: return 16;
#if ! UNITY_IPHONE
        case TextureFormat.DXT1Crunched: return 4;
        case TextureFormat.DXT5Crunched: return 8;
#endif
        case TextureFormat.PVRTC_RGB2: return 2;
        case TextureFormat.PVRTC_RGBA2: return 2;
        case TextureFormat.PVRTC_RGB4: return 4;
        case TextureFormat.PVRTC_RGBA4: return 4;
        case TextureFormat.ETC_RGB4: return 4;
        case TextureFormat.EAC_R: return 4;
        case TextureFormat.EAC_R_SIGNED: return 4;
        case TextureFormat.EAC_RG: return 8;
        case TextureFormat.EAC_RG_SIGNED: return 8;
        case TextureFormat.ETC2_RGB: return 4;
        case TextureFormat.ETC2_RGBA1: return 4;
        case TextureFormat.ETC2_RGBA8: return 8;
        case TextureFormat.ASTC_RGB_4x4: return 8;
        case TextureFormat.ASTC_RGB_5x5: return 5.12f;
        case TextureFormat.ASTC_RGB_6x6: return 3.56f;
        case TextureFormat.ASTC_RGB_8x8: return 2;
        case TextureFormat.ASTC_RGB_10x10: return 1.28f;
        case TextureFormat.ASTC_RGB_12x12: return 0.89f;
        case TextureFormat.ASTC_RGBA_4x4: return 8;
        case TextureFormat.ASTC_RGBA_5x5: return 5.12f;
        case TextureFormat.ASTC_RGBA_6x6: return 3.56f;
        case TextureFormat.ASTC_RGBA_8x8: return 2;
        case TextureFormat.ASTC_RGBA_10x10: return 1.28f;
        case TextureFormat.ASTC_RGBA_12x12: return 0.89f;
        case TextureFormat.ETC_RGB4_3DS: return 4;
        case TextureFormat.ETC_RGBA8_3DS: return 8;
        case TextureFormat.BC4: return 4;
        case TextureFormat.BC5: return 8;
        case TextureFormat.BC6H: return 8;
        case TextureFormat.BC7: return 8;
        case TextureFormat.RGB9e5Float: return 9 * 3 + 5;
        case TextureFormat.RG16: return 16;
        case TextureFormat.R8: return 8;
#if !UNITY_IOS
        case TextureFormat.ETC_RGB4Crunched: return 4;
        case TextureFormat.ETC2_RGBA8Crunched: return 8;
#endif
        default:
          throw new ArgumentOutOfRangeException(nameof(tf), tf, null);
      }
      #pragma warning restore SwitchEnumAnalyzer
    }
  }
}