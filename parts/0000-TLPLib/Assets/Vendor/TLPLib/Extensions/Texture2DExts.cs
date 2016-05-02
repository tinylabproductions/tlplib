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
  }
}
