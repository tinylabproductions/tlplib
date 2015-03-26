using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class PinchObservable : MonoBehaviour {
    readonly Subject<Tpl<float, float>> _pinchDelta = new Subject<Tpl<float, float>>();
    public IObservable<Tpl<float, float>> pinchDelta { get { return _pinchDelta; } }

    ISubscription sub;

    internal void Awake() {
      // TODO: it makes no sense for this to be in MonoBehaviour
      sub = Observable.touches.subscribe(touches => {
        if (touches.Count >= 2) {
          var touchZero = touches[touches.Count - 2];
          var touchOne = touches[touches.Count - 1];

          var prevTouchDistance = (touchZero.previousPosition - touchOne.previousPosition).magnitude;
          var touchDistance = (touchZero.position - touchOne.position).magnitude;

          var deltaMagnitudeDiff = prevTouchDistance - touchDistance;

          _pinchDelta.push(F.t(prevTouchDistance, deltaMagnitudeDiff));
        }
      });
    }

    internal void Destroy() {
      sub.unsubscribe();
    }
  }
}