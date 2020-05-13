using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  public static class SerializedEasePreview {
#if UNITY_EDITOR
    static Dictionary<SimpleSerializedEase, Texture2D> _imagesCache = new Dictionary<SimpleSerializedEase, Texture2D>();

    public static Texture2D editorPreview(SimpleSerializedEase simple) => 
      // ReSharper disable once ConvertClosureToMethodGroup
      _imagesCache.getOrUpdate(simple, simple_ => generateTexture(simple_.toEase()));

    public static Texture2D generateTexture(Ease ease) {
      const float VERTICAL_OFFSET = 0.25f;
      const int SIZE = 46 * 2;
      var texture = new Texture2D(SIZE, SIZE);
      texture.fill(Color.black);
      for (var _x = 0; _x < texture.width; _x++) {
        var _y = (int) (
          ease.Invoke(_x / (float) texture.width)
          * texture.height
          * (1f - VERTICAL_OFFSET * 2f) 
          + VERTICAL_OFFSET * texture.height
        );
          
        texture.SetPixel(_x, (int) (VERTICAL_OFFSET * texture.height), Color.gray);
        texture.SetPixel(_x, (int) ((1f - VERTICAL_OFFSET) * texture.height), Color.gray);
          
        // 2x2 pixel
        for (var x = _x; x < _x + 2 && x < texture.width; x++) {
          for (var y = _y; y < _y + 2 && y < texture.height; y++) {
            texture.SetPixel(x, y, Color.green);
          }
        }
      }
      texture.Apply();
      return texture;
    }
#else
    public static Texture2D editorPreview(SimpleSerializedEase simple) => null;
#endif
  }
}
