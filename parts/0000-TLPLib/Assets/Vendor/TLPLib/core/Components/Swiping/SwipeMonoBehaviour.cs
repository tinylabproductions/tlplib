using com.tinylabproductions.TLPLib.Components.Interfaces;
using pzd.lib.reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Swiping {
  public enum SwipeDirection {
    Left, Right, Up, Down
  }

  public class SwipeMonoBehaviour : MonoBehaviour, IMB_Awake, IBeginDragHandler, IDragHandler {
#pragma warning disable 649
    [SerializeField] float threshold = 30;
#pragma warning restore 649

    Vector2 dragBeginPos;
    RectTransform rt;

    readonly Subject<SwipeDirection> swipeAction = new Subject<SwipeDirection>();
    public IRxObservable<SwipeDirection> swipe => swipeAction;

    public void Awake() {
      rt = GetComponent<RectTransform>();
      if (threshold <= 0) threshold = 1;
    }

    static Vector2 screenToLocal(RectTransform rt, PointerEventData eventData) {
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
        rt, eventData.position, eventData.pressEventCamera, out var localPoint
      );
      return localPoint;
    }

    public void OnBeginDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      dragBeginPos = screenToLocal(rt, eventData);
    }

    public void OnDrag(PointerEventData eventData) {
      if (!mayDrag(eventData)) return;
      var current = screenToLocal(rt, eventData);
      
      while (current.x - dragBeginPos.x > threshold) {
        push(SwipeDirection.Right, eventData);
        dragBeginPos.x += threshold;
      }
      while (current.x - dragBeginPos.x < -threshold) {
        push(SwipeDirection.Left, eventData);
        dragBeginPos.x -= threshold;
      }
      while (current.y - dragBeginPos.y > threshold) {
        push(SwipeDirection.Up, eventData);
        dragBeginPos.y += threshold;
      }
      while (current.y - dragBeginPos.y < -threshold) {
        push(SwipeDirection.Down, eventData);
        dragBeginPos.y -= threshold;
      }
    }

    void push(SwipeDirection dir, PointerEventData eventData) {
      eventData.eligibleForClick = false;
      swipeAction.push(dir);
    }

    static bool mayDrag(PointerEventData eventData) =>
      eventData.button == PointerEventData.InputButton.Left;
  }
}