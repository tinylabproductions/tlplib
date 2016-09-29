using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class Texture2DExts {
    public static void fill(this Texture2D texture, Color color) {
      var fillColorArray = texture.GetPixels();
      for (var i = 0; i < fillColorArray.Length; ++i)
        fillColorArray[i] = color;
     
      texture.SetPixels(fillColorArray);
      texture.Apply();
    }

    public static void invert(this Color[] pixels, Color[] pixelsTo=null) {
      pixelsTo = pixelsTo ?? pixels;
      for (var idx = 0; idx < pixels.Length; idx++) {
        var current = pixels[idx];
        var inverted = new Color(1 - current.r, 1 - current.g, 1 - current.b, current.a);
        pixelsTo[idx] = inverted;
      }
    }

    public static Color[] inverted(this Color[] pixels) {
      var newPixels = new Color[pixels.Length];
      pixels.invert(newPixels);
      return newPixels;
    }

    public static void invert(this Texture2D texture) {
      var pixels = texture.GetPixels();
      pixels.invert();
      texture.SetPixels(pixels);
      texture.Apply();
    }

    public static Texture2D newWithSameAttrs(this Texture2D texture, Color[] pixels=null) {
      var t = new Texture2D(
        texture.width, texture.height, texture.format, texture.mipmapCount != 0
      );
      if (pixels != null) {
        t.SetPixels(pixels);
        t.Apply();
      }
      return t;
    }
  }
}
