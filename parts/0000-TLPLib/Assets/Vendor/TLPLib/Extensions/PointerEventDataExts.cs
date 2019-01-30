using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class PointerEventDataExts {
    public static Vector2 screenPointToLocalPointInRectangle(this PointerEventData eventData, RectTransform parent) {
      Vector2 startMousePos;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
        parent, eventData.position, eventData.pressEventCamera, out startMousePos
      );
      return startMousePos;
    }
  }
}
