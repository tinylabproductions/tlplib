using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI {
  public class SlideTickerForwarder : MonoBehaviour, IBeginDragHandler, IDragHandler {
    public float tickDistance;

    readonly Subject<Tpl<int, int>> _onSlideTick = new Subject<Tpl<int, int>>();
    public IObservable<Tpl<int, int>> onSlideTick => _onSlideTick;
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
        _onSlideTick.push(F.t(tickX, tickY));
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