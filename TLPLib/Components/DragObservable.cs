using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class DragObservable : MonoBehaviour {
    public static readonly float dragThresholdSqr =
      Mathf.Pow(ScreenUtils.cmToPixels(0.25f), 2);
    private const int MOUSE_BUTTON = 0;

    private readonly Subject<Vector2> _dragDelta = new Subject<Vector2>();
    public IObservable<Vector2> dragDelta { get { return _dragDelta; } }

    private Option<Vector2> lastDragPosition = F.none<Vector2>();
    private bool dragStarted;

    [UsedImplicitly]
    private void Update() {
      if (lastDragPosition.isEmpty) {
        if (isMouseDown()) {
          lastDragPosition = F.some((Vector2) Input.mousePosition);
          dragStarted = false;
        }
      }
      else {
        if (isMouseUp()) {
          lastDragPosition = F.none<Vector2>();
          return;
        }

        var lastPos = lastDragPosition.get;
        var curPos = (Vector2) Input.mousePosition;
        if (!dragStarted && (curPos - lastPos).sqrMagnitude >= dragThresholdSqr) {
          dragStarted = true;
        }
        if (dragStarted && curPos != lastPos) {
          _dragDelta.push(curPos - lastPos);
          lastDragPosition = F.some(curPos);
        }
      }
    }

    static bool isMouseDown() { return Input.touchCount <= 1 && Input.GetMouseButtonDown(MOUSE_BUTTON); }

    static bool isMouseUp() {
      return Input.touchCount > 1 || Input.GetMouseButtonUp(MOUSE_BUTTON)
        || Input.GetMouseButton(MOUSE_BUTTON) == false;
    }
  }
}
