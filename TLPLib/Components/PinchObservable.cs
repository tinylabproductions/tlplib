using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class PinchObservable : MonoBehaviour {
    private readonly Subject<float> _pinchDelta = new Subject<float>();
    public IObservable<float> pinchDelta { get { return _pinchDelta; } }

    [UsedImplicitly]
    private void Update() {
      if (Input.touchCount == 2) {
        var touchZero = Input.GetTouch(0);
        var touchOne = Input.GetTouch(1);

        var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        var prevTouchDistance = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        var touchDistance = (touchZero.position - touchOne.position).magnitude;

        var deltaMagnitudeDiff = prevTouchDistance - touchDistance;

        _pinchDelta.push(deltaMagnitudeDiff);
      }
    }
  }
}
