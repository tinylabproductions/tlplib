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

    public static Sprite toSprite(this Texture2D texture, float pixelsPerUnit = 100) {
      var rec = new Rect(0, 0, texture.width, texture.height);
      return Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    public static Texture2D textureFromCamera(
      this Camera camera, int width, int height,
      int depthBuffer = 16,
      RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32,
      TextureFormat textureFormat = TextureFormat.RGB24,
      bool mipmap = false
    ) {
      var renderTexture = RenderTexture.GetTemporary(width, height, depthBuffer, renderTextureFormat);
      renderTexture.Create();
      var prevTargetTexture = camera.targetTexture;
      camera.targetTexture = renderTexture;
      camera.Render();
      camera.targetTexture = prevTargetTexture;

      /*
       * Texture2D#ReadPixels reads all pixels on the screen if RenderTexture.active is null,
       * otherwise it reads from the set texture.
       * https://docs.unity3d.com/ScriptReference/RenderTexture-active.html
       */
      var currentRT = RenderTexture.active;
      RenderTexture.active = renderTexture;
      var texture = new Texture2D(width, height, textureFormat, mipmap);
      texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
      texture.Apply();
      RenderTexture.active = currentRT;
      RenderTexture.ReleaseTemporary(renderTexture);
      return texture;
    }

    // Well, sometimes SupportsTextureFormat throws an exception. Not much we can do there.
    public static bool isSupported(this TextureFormat f) {
      try { return SystemInfo.SupportsTextureFormat(f); }
      catch { return false; }
    }

    public static bool isSupported(this RenderTextureFormat f) {
      try { return SystemInfo.SupportsRenderTextureFormat(f); }
      catch { return false; }
    }
  }
}
