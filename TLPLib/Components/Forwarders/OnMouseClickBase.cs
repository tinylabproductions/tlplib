using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public abstract class OnPointerClickBase : MonoBehaviour {
    private bool uguiBlocks;
    private Option<Vector3> downPosition;

    protected void init(bool uguiBlocks) {
      PointerHandlerBehaviour.init();
      this.uguiBlocks = uguiBlocks;
    }

    public void onPointerDown(Vector3 hitPoint) {
      if (uguiBlocks && (
        // Mouse
        EventSystem.current.IsPointerOverGameObject(-1) ||
        // Touch 0 - will not exist in EventSystem in onPointerUp.
        (Input.touchCount >= 1 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
      )) {
        Log.trace("ugui blocked onPointerDown @ " + hitPoint);
        return;
      }

      downPosition = F.some(Camera.main.WorldToScreenPoint(hitPoint));
    }

    public void onPointerUp(Vector3 hitPoint) {
      if (downPosition.isEmpty) return;

      var startPos = downPosition.get;
      var diff = Camera.main.WorldToScreenPoint(hitPoint) - startPos;
      if (diff.sqrMagnitude < DragObservable.dragThresholdSqr) pointerClick(hitPoint);

      downPosition = F.none<Vector3>();
    }

    protected abstract void pointerClick(Vector3 hitPoint);
  }
}
