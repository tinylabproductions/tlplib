using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.InputUtils;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class DragObservable : MonoBehaviour {
    public static readonly float dragThresholdSqr =
      Mathf.Pow(ScreenUtils.cmToPixels(0.25f), 2);

    private readonly Subject<Vector2> _dragDelta = new Subject<Vector2>();
    public IObservable<Vector2> dragDelta { get { return _dragDelta; } }

    private Option<Vector2> lastDragPosition = F.none<Vector2>();
    private bool dragStarted;

    [UsedImplicitly]
    private void Update() {
      if (lastDragPosition.isEmpty) {
        if (Pointer.isDown) {
          Log.trace("drag pointer down");
          lastDragPosition = F.some(Pointer.currentPosition);
          dragStarted = false;
        }
      }
      else {
        if (Pointer.isUp) {
          Log.trace("drag pointer up");
          lastDragPosition = F.none<Vector2>();
          return;
        }

        var lastPos = lastDragPosition.get;
        var curPos = Pointer.currentPosition;
        if (!dragStarted && (curPos - lastPos).sqrMagnitude >= dragThresholdSqr) {
          dragStarted = true;
        }
        if (dragStarted && curPos != lastPos) {
          var delta = curPos - lastPos;
          _dragDelta.push(delta);
          lastDragPosition = F.some(curPos);
        }
      }
    }
  }
}
