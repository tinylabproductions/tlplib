using System;
using AdvancedInspector;
using com.tinylabproductions.TLPGame.unity_serialization;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Swiping {
  public class SwipeMonoBehaviour : MonoBehaviour, IMB_Awake, IBeginDragHandler, IEndDragHandler, IDragHandler {
#pragma warning disable 649
    [
      SerializeField,
      Help(HelpType.Info, "Events are emitted on release.\n" +
                          "If set, event will be emiited once drag threshold is reached instead. " +
                          "value is in screen coordinates.")
    ] UnityOptionFloat eventOnThreshold;
#pragma warning restore 649

    public event Action<Vector2> swipeEnded;
    public event Action swipedLeft;
    public event Action swipedRigth;
    public event Action swipedUp;
    public event Action swipedDown;
    Vector2 dragBeginPos;
    bool dragFinished;
    Option<float> _eventOnThresholdSqr;
    RectTransform rt;

    public void Awake() {
      _eventOnThresholdSqr = eventOnThreshold.value.map(_ => _ * _);
      rt = GetComponent<RectTransform>();
    }

    Vector2 screenToLocal(RectTransform rt, PointerEventData eventData) {
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
      if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
        if (delta.x > 0) swipedRigth?.Invoke();
        else swipedLeft?.Invoke();
      }
      else {
        if (delta.y > 0) swipedUp?.Invoke();
        else swipedDown?.Invoke();
      }
    }

    static bool mayDrag(PointerEventData eventData) => eventData.button == PointerEventData.InputButton.Left;
  }
}