using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class SpriteRendererUtils {
    public static Option<Rect> calculateSpriteBounds(List<SpriteRenderer> spriteRenderers) {
      var hasOne = false;
      var b = default(Rect);
      foreach (var r in spriteRenderers) {
        var sprite = r.sprite;
        if (!sprite) continue;
        var scale = r.transform.lossyScale;
        var pixelsPerUnit = sprite.pixelsPerUnit;
        var rect = sprite.rect;
        var size = new Vector2(rect.width / pixelsPerUnit, rect.height / pixelsPerUnit).multiply(scale);
        var offset = ((rect.size / 2 - sprite.pivot) / pixelsPerUnit).multiply(scale);
        var newBounds = RectUtils.fromCenter((Vector2) r.transform.position + offset, size);
        if (hasOne) {
          b = b.encapsulate(newBounds);
        }
        else {
          hasOne = true;
          b = newBounds;
        }
      }
      return hasOne.opt(b);
    }
  }
}