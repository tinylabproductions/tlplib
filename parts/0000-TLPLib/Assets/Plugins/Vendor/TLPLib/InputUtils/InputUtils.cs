using UnityEngine;

namespace com.tinylabproductions.TLPLib.InputUtils {
  /* Abstracts away mouse & touch handling */
  public static class Pointer {
    public const int MOUSE_BTN_FIRST = 0;

    public static bool isDown { get {
      return (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) ||
             Input.GetMouseButtonDown(MOUSE_BTN_FIRST);
    } }

    public static bool held { get {
      return Input.touchCount == 1 || Input.GetMouseButton(MOUSE_BTN_FIRST);
    } }

    public static bool isUp { get {
      return Input.touchCount != 1 && Input.GetMouseButton(MOUSE_BTN_FIRST) == false;
    } }

    public static Vector2 currentPosition { get { return Input.mousePosition; } }
  }
}