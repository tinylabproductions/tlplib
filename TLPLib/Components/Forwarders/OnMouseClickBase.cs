using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public abstract class OnMouseClickBase : MonoBehaviour {
    private bool uguiBlocks;
    private Option<Vector3> downPosition;
    protected new Collider collider;

    protected void init(bool uguiBlocks) {
      this.uguiBlocks = uguiBlocks;
      collider = GetComponent<Collider>();
    }

    [UsedImplicitly]
    private void OnMouseDown() {
      if (uguiBlocks && EventSystem.current.IsPointerOverGameObject())
        return;

      downPosition = F.some(Input.mousePosition);

      ASync.EveryFrame(this, () => {
        if (downPosition.isEmpty) return false;
        var startPos = downPosition.get;
        var diff = Input.mousePosition - startPos;
        if (diff.sqrMagnitude >= DragObservable.dragThresholdSqr) {
          downPosition = F.none<Vector3>();
          return false;
        }
        return true;
      });
    }

    [UsedImplicitly]
    private void OnMouseUp() {
      if (downPosition.isEmpty) return;
      if (uguiBlocks && EventSystem.current.IsPointerOverGameObject()) return;
      mouseClick();
      downPosition = F.none<Vector3>();
    }

    protected abstract void mouseClick();
  }
}
