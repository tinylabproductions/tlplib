using System;

using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.unity_serialization;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Swiping {
  public enum SwipeDirection {
    Left, Right, Up, Down
  }

  public class SwipeMonoBehaviour : MonoBehaviour, IMB_Awake, IBeginDragHandler, IEndDragHandler, IDragHandler {
#pragma warning disable 649
    [
      SerializeField,
      // Help(HelpType.Info, "Events are emitted on release.\n" +
      //                     "If set, event will be emitted once drag threshold is reached instead. " +
      //                     "value is in screen coordinates.")
    ] UnityOptionFloat eventOnThreshold;
#pragma warning restore 649

    public event Action<Vector2> swipeEnded;
    public event Action swipedLeft, swipedRigth, swipedUp, swipedDown;

    Vector2 dragBeginPos;
    bool dragFinished;
    Option<float> _eventOnThresholdSqr;
    RectTransform rt;

    readonly Subject<SwipeDirection> swipeAction = new Subject<SwipeDirection>();
    public IRxObservable<SwipeDirection> swipe => swipeAction;

    public void Awake() {
      _eventOnThresholdSqr = eventOnThreshold.value.map(_ => _ * _);
      rt = GetComponent<RectTransform>();
    }

    static Vector2 screenToLocal(RectTransform rt, PointerEventData eventData) {
      Vector2 localPoint;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, eventData.position, eventData.pressEventCamera, out localPoint
      );
      return localPoint;
    }

    public void OnBeginDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      dragFinished = false;
      dragBeginPos = screenToLocal(rt, eventData);
    }

    public void OnEndDrag(PointerEventData eventData) {
      if (_eventOnThresholdSqr.isSome) return;
      if (!mayDrag(eventData)) return;
      if (dragFinished) return;
      finishSwipe(screenToLocal(rt, eventData) - dragBeginPos);
    }

    public void OnDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      if (dragFinished) return;
      foreach (var thresholdSqr in _eventOnThresholdSqr) {
        var delta = screenToLocal(rt, eventData) - dragBeginPos;
        if (delta.sqrMagnitude >= thresholdSqr) {
          // if we have UIClickForwarder on the same object we want to skip the click event on that
          eventData.eligibleForClick = false;
          finishSwipe(delta);
        }
      }
    }

    void finishSwipe(Vector2 delta) {
      dragFinished = true;
      swipeEnded?.Invoke(delta);
      var swipeDir = getSwipeDirection(delta);
      swipeAction.push(swipeDir);
      switch (swipeDir) {
        case SwipeDirection.Left:  swipedLeft?.Invoke();  break;
        case SwipeDirection.Right: swipedRigth?.Invoke(); break;
        case SwipeDirection.Up:    swipedUp?.Invoke();    break;
        case SwipeDirection.Down:  swipedDown?.Invoke();  break;
        default: throw new ArgumentOutOfRangeException();
      }
    }

    public static SwipeDirection getSwipeDirection(Vector2 delta) =>
      (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        ? delta.x > 0
          ? SwipeDirection.Right
          : SwipeDirection.Left
        : delta.y > 0
          ? SwipeDirection.Up
          : SwipeDirection.Down;

    static bool mayDrag(PointerEventData eventData) =>
      eventData.button == PointerEventData.InputButton.Left;
  }
}