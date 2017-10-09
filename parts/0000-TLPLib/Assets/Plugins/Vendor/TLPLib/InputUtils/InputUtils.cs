using UnityEngine;

namespace com.tinylabproductions.TLPLib.InputUtils {
  /* Abstracts away mouse & touch handling */
  public static class Pointer {
    public const int MOUSE_BTN_FIRST = 0;

    public static bool isDown => 
      (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) 
      || Input.GetMouseButtonDown(MOUSE_BTN_FIRST);

    public static bool held => Input.touchCount == 1 || Input.GetMouseButton(MOUSE_BTN_FIRST);

    public static bool isUp => Input.touchCount != 1 && Input.GetMouseButton(MOUSE_BTN_FIRST) == false;

    public static Vector2 currentPosition => Input.mousePosition;
  }
}