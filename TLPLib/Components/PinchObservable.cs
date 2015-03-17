using System;
using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class PinchObservable : MonoBehaviour {
    private readonly Subject<Tpl<float, float>> _pinchDelta = new Subject<Tpl<float, float>>();
    public IObservable<Tpl<float, float>> pinchDelta { get { return _pinchDelta; } }

    [UsedImplicitly]
    private void Update() {
      if (Input.touchCount >= 2) {
        var touchZero = Input.GetTouch(Input.touchCount - 2);
        var touchOne = Input.GetTouch(Input.touchCount - 1);

        var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        var prevTouchDistance = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        var touchDistance = (touchZero.position - touchOne.position).magnitude;

        var deltaMagnitudeDiff = prevTouchDistance - touchDistance;

        _pinchDelta.push(F.t(prevTouchDistance, deltaMagnitudeDiff));
      }
    }
  }
}
