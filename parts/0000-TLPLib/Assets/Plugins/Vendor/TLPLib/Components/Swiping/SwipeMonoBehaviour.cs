using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Swiping {
  public class SwipeMonoBehaviour : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, ISwipeEventSource {
    public event Action<Vector2> swipeEnded;
    public event Action swipedLeft;
    public event Action swipedRigth;
    public event Action swipedUp;
    public event Action swipedDown;
    Vector2 dragBeginPos;

    public void OnBeginDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      dragBeginPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      var delta = eventData.position - dragBeginPos;
      swipeEnded?.Invoke(delta);
      if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
        if (delta.x > 0) {
          swipedLeft?.Invoke();
        }
        else {
          swipedRigth?.Invoke();
        }
      }
      else {
        if (delta.y > 0) {
          swipedDown?.Invoke();
        }
        else {
          swipedUp?.Invoke();
        }
      }
    }

    public void OnDrag(PointerEventData eventData) {
      // Required for BeginDrag / EndDrag to work
    }

    static bool mayDrag(PointerEventData eventData) { return eventData.button == PointerEventData.InputButton.Left; }
  }
}