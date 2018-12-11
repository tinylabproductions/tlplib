using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class SlideTickerForwarder : MonoBehaviour, IBeginDragHandler, IDragHandler {
    public float tickDistance;

    readonly Subject<Point2D> _onSlideTick = new Subject<Point2D>();
    public IRxObservable<Point2D> onSlideTick => _onSlideTick;
    Vector2 dragBeginPos;

    public void OnBeginDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      dragBeginPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      var delta = eventData.position - dragBeginPos;
      var tickX = calcAxisTicks(delta.x, tickDistance);
      var tickY = calcAxisTicks(delta.y, tickDistance);
      if (tickX != 0 || tickY != 0) {
        dragBeginPos += new Vector2(tickX * tickDistance, tickY * tickDistance);
        _onSlideTick.push(new Point2D(tickX, tickY));
      }
    }

    static int calcAxisTicks(float delta, float tickDistance) {
      var ticks = Mathf.FloorToInt(Mathf.Abs(delta) / tickDistance);
      return delta >= 0 ? ticks : -ticks;
    }

    static bool mayDrag(PointerEventData eventData) =>
      eventData.button == PointerEventData.InputButton.Left;
  }
}