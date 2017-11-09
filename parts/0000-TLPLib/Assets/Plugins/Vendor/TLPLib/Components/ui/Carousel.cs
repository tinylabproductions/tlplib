using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Components.Swiping;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.unity_serialization;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.ui {
  public class Carousel : UIBehaviour, IMB_Update {
    public enum Direction : byte { Horizontal = 0, Vertical = 1 }

    #region Unity Serialized Fields

#pragma warning disable 649
    public float
      SpaceBetweenSelectedAndAdjacentPages,
      SpaceBetweenOtherPages,
      SelectedPageItemsScale = 1,
      OtherPagesItemsScale = 1,
      AdjacentToSelectedPageItemsScale = 1,
      moveCompletedEventThreshold = 0.02f;
    public bool wrapCarouselAround;
    [SerializeField] UnityOptionInt maxElementsFromCenter;
    [SerializeField] UnityOptionVector3 selectedPageOffset;
    // ReSharper disable once NotNullMemberIsNotInitialized
    [SerializeField] Direction _direction = Direction.Horizontal;
#pragma warning restore 649

    #endregion

    List<GameObject> _elements = new List<GameObject>();
    public List<GameObject> elements {
      get { return _elements; }
      set {
        _elements = value;
        initPositions();
      }
    }

    // disables elements for which position from center exceeds this value
    public Option<float> disableDistantElements = F.none<float>();
    public bool loopable => wrapCarouselAround && elements.Count > 4;
    readonly RxRef<int> _page = new RxRef<int>(0);
    public IRxVal<int> page => _page;
    float currentPosition;

    int targetPageValue;
    public bool isMoving { get; private set; }
    readonly Subject<Unit> _movementComplete = new Subject<Unit>();
    public IObservable<Unit> movementComplete => _movementComplete;
    public Direction direction => _direction;

    public void nextPage() => setPageAnimated(targetPageValue + 1);
    public void prevPage() => setPageAnimated(targetPageValue - 1);

    /// <summary>Set page without any animations.</summary>
    public void setPageInstantly(int index) {
      currentPosition = index;
      targetPageValue = index;
      _page.value = index;
    }

    /// <summary>Set page with smooth animations.</summary>
    public void setPageAnimated(int index) {
      if (!loopable) {
        // when we increase past last page go to page 0 if wrapCarouselAround == true
        targetPageValue = wrapCarouselAround 
          ? index.modPositive(elements.Count) 
          : Mathf.Clamp(index, 0, elements.Count - 1);
        _page.value = targetPageValue;
      }
      else {
        var diff = Mathf.Abs(index - targetPageValue);
        if (diff < 2) {
          targetPageValue = Mathf.Clamp(index, -1, elements.Count);
          if (index != targetPageValue) {
            currentPosition = (elements.Count + targetPageValue) %elements.Count;
            targetPageValue = (elements.Count + index)%elements.Count;
          }
          _page.value = (elements.Count + targetPageValue)%elements.Count;
        }
        else {
          if (diff > elements.Count/2f) {
            currentPosition = targetPageValue + (targetPageValue < index ? elements.Count : -elements.Count);
          }
          targetPageValue = (elements.Count + index) % elements.Count;
          _page.value = targetPageValue;
        }
      }
    }

    public void Update() {
      lerpPosition(Time.deltaTime * 5);
    } 

    void lerpPosition(float amount) {
      var withinMoveCompletedThreshold =
        Math.Abs(currentPosition - targetPageValue) < moveCompletedEventThreshold;
      if (isMoving && withinMoveCompletedThreshold) _movementComplete.push(F.unit);
      isMoving = !withinMoveCompletedThreshold;

      currentPosition = Mathf.Lerp(currentPosition, targetPageValue, amount);
      var isMovingLeft = targetPageValue < currentPosition;
      var elementsOnTheLeft = _elements.Count / 2 + (isMovingLeft ? -1 : 0);

      for (var idx = 0; idx < elements.Count; idx++) {
        var elementPos = currentPosition;
        if (loopable) {
          if (targetPageValue > idx + elementsOnTheLeft) {
            elementPos = currentPosition - elements.Count;
          }
          if (targetPageValue + elements.Count - 1 < idx + elementsOnTheLeft) {
            elementPos = currentPosition + elements.Count;
          }
        }
        var absDiff = Mathf.Abs(idx - elementPos);
        var sign = Mathf.Sign(idx - elementPos);
        var delta = (Mathf.Clamp01(absDiff) 
          * SpaceBetweenSelectedAndAdjacentPages + Mathf.Max(0, absDiff - 1) 
          * SpaceBetweenOtherPages) 
          * sign;

        foreach (var distance in disableDistantElements) {
          elements[idx].SetActive(Mathf.Abs(delta) < distance);
        }

        var t = elements[idx].transform;
        t.localPosition = getPosition(_direction, delta, absDiff, selectedPageOffset);

        t.localScale = Vector3.one * (absDiff < 1
          ? Mathf.Lerp(SelectedPageItemsScale, OtherPagesItemsScale, absDiff)
          : 
            maxElementsFromCenter.value.fold(
              () => Mathf.Lerp(OtherPagesItemsScale, AdjacentToSelectedPageItemsScale, absDiff - 1),
              maxElementsFromCenter => Mathf.Lerp(
                AdjacentToSelectedPageItemsScale, 0, absDiff - maxElementsFromCenter
              )
            )
        );
      }
    }

    static Vector3 getPosition(
      Direction carouselDirection, float positionChange, float absDiff, 
      Option<Vector3> centralItemOffset 
    ) {
      var newPos = carouselDirection == Direction.Horizontal 
        ? new Vector3(positionChange, 0, 0)
        : new Vector3(0, -positionChange, 0);

      foreach (var offset in centralItemOffset) {
        var lerpedOffset = Vector3.Lerp(offset, Vector3.zero, absDiff);
        return newPos + lerpedOffset;
      }

      return newPos;
    }

    void initPositions() => lerpPosition(1);

    public void handleCarouselSwipe(SwipeDirection swipeDirection) {
      switch (direction) {
        case Direction.Horizontal:
          switch (swipeDirection) {
            case SwipeDirection.Left:
              nextPage();
              break;
            case SwipeDirection.Right:
              prevPage();
              break;
            case SwipeDirection.Up:
            case SwipeDirection.Down:
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(swipeDirection), swipeDirection, null);
          }
          break;
        case Direction.Vertical:
          switch (swipeDirection) {
            case SwipeDirection.Left:
            case SwipeDirection.Right:
              break;
            case SwipeDirection.Up:
              nextPage();
              break;
            case SwipeDirection.Down:
              prevPage();
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(swipeDirection), swipeDirection, null);
          }
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
      }
    }
  }
}