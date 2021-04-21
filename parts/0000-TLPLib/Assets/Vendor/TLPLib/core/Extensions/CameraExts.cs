using UnityEngine;

namespace Vendor.TLPLib.core.Extensions {
  public static class CameraExts {
    public static Vector2 worldPosToCanvasAnchoredPosition(
      this Camera camera, Vector3 worldPos, RectTransform canvasTransform
    ) {
      var rect = canvasTransform.rect;
      var viewport = camera.WorldToViewportPoint(worldPos);
      var screenPosition = new Vector2(rect.width * viewport.x, rect.height * viewport.y);
      return screenPosition;
    }
  }
}