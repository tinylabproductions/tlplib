using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Swiping {
  public class SwipeMonoBehaviour : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
    public event Action<Vector2> swipeEnded;
    public event Action swipedLeft, swipedRigth, swipedUp, swipedDown;

    Vector2 dragBeginPos;

    public void OnBeginDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      dragBeginPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;

      var delta = eventData.position - dragBeginPos;
      swipeEnded?.Invoke(delta);
      if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        (delta.x > 0 ? swipedLeft : swipedRigth)?.Invoke();
      else
        (delta.y > 0 ? swipedDown : swipedUp)?.Invoke();
    }

    public void OnDrag(PointerEventData eventData) {
      // Required for BeginDrag / EndDrag to work
    }

    static bool mayDrag(PointerEventData eventData) => 
      eventData.button == PointerEventData.InputButton.Left;
  }
}