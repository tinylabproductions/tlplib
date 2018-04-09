using System;
using System.Collections.Generic;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Components.Swiping;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.unity_serialization;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.ui {
  public class Carousel : Carousel<CarouselGameObject> {
    public enum Direction : byte { Horizontal = 0, Vertical = 1 }
  }

  public interface ICarouselItem {
    GameObject gameObject { get; }
  }

  public class Carousel<A> : UIBehaviour, IMB_Update, IMB_OnDrawGizmosSelected where A : ICarouselItem {

    #region Unity Serialized Fields

#pragma warning disable 649
    public float
      SpaceBetweenSelectedAndAdjacentPages,
      SpaceBetweenOtherPages,
      SelectedPageItemsScale = 1,
      OtherPagesItemsScale = 1,
      AdjacentToSelectedPageItemsScale = 1,
      moveCompletedEventThreshold = 0.02f;
    public bool wrapCarouselAround, whenNotLoopableMove = true;
    [SerializeField] UnityOptionInt maxElementsFromCenter;
    [SerializeField] UnityOptionVector3 selectedPageOffset;
    // ReSharper disable once NotNullMemberIsNotInitialized
    [SerializeField] Carousel.Direction _direction = Carousel.Direction.Horizontal;
    // There's still a visual issue when all items fits into selection window
    [SerializeField] float selectionWindowWidth;
#pragma warning restore 649

    #endregion

    public Option<int> maxElementsFromCenterOpt => maxElementsFromCenter.value;

    readonly List<A> elements = new List<A>();

    /// <summary>
    /// Updates visual if we mutate elements.
    /// Done this way because it's more performant than immutable version
    /// </summary>
    public void editElements(Act<List<A>> f, bool animate = false) {
      f(elements);
      var pageValue = Mathf.Clamp(_page.value, 0, elements.Count - 1);
      if (animate)
        setPageAnimated(pageValue);
      else
        setPageInstantly(pageValue);
      updateCurrentElement();
    }

    public int elementsCount => elements.Count;

    // disables elements for which position from center exceeds this value
    [ReadOnly] public Option<float> disableDistantElements = F.none<float>();
    bool loopable => wrapCarouselAround && elements.Count > 4;

    readonly RxRef<int> _page = new RxRef<int>(0);
    public IRxVal<int> page => _page;

    readonly LazyVal<IRxRef<Option<A>>> __currentElement;
    public IRxVal<Option<A>> currentElement => __currentElement.strict;

    void updateCurrentElement() {
      if (__currentElement.isCompleted) {
        __currentElement.strict.value = elements.get(_page.value);
      }
    }

    float currentPosition, pivotState;

    int targetPageValue;
    public bool isMoving { get; private set; }
    readonly Subject<Unit> _movementComplete = new Subject<Unit>();
    public IObservable<Unit> movementComplete => _movementComplete;

    public void nextPage() => movePagesByAnimated(1);
    public void prevPage() => movePagesByAnimated(-1);

    protected Carousel() {
      __currentElement = F.lazy(() => {
        var res = RxRef.a(elements.get(page.value));
        _page.subscribe(gameObject, p => res.value = elements.get(p));
        return res;
      });
    }

    /// <summary>Set page without any animations.</summary>
    public void setPageInstantly(int index) {
      currentPosition = index;
      targetPageValue = index;
      _page.value = index;
    }

    /// <summary>Set page with smooth animations.</summary>
    public void setPageAnimated(int page) {
      if (elements.isEmpty()) return;
      var current = page - _page.value;
      void findBestPage(int p) {
        if (Math.Abs(p + pivotState) <= Math.Abs(current + pivotState)) {
          current = p;
        }
      }
      findBestPage(page - _page.value - elementsCount);
      findBestPage(page - _page.value + elementsCount);
      movePagesByAnimated(current);
    }

    void movePagesByAnimated(int offset) {
      if (elements.isEmpty()) return;

      if (!loopable) {
        // when we increase past last page go to page 0 if wrapCarouselAround == true
        var page = offset + targetPageValue;
        targetPageValue = wrapCarouselAround
          ? page.modPositive(elements.Count)
          : Mathf.Clamp(page, 0, elements.Count - 1);
        _page.value = targetPageValue;
      }
      else {
        targetPageValue += offset;
        _page.value = targetPageValue.modPositive(elements.Count);
      }
    }

    public void Update() {
      lerpPosition(Time.deltaTime * 5);
    }

    void lerpPosition(float amount) {
      if (elements.isEmpty()) return;

      var withinMoveCompletedThreshold =
        Math.Abs(currentPosition - targetPageValue) < moveCompletedEventThreshold;
      if (isMoving && withinMoveCompletedThreshold) _movementComplete.push(F.unit);
      isMoving = !withinMoveCompletedThreshold;

      var prevPos = currentPosition;
      currentPosition = Mathf.Lerp(currentPosition, targetPageValue, amount);
      var posDiff = currentPosition - prevPos;

      while (currentPosition > elementsCount) {
        currentPosition -= elementsCount;
        targetPageValue -= elementsCount;
      }

      while (currentPosition < 0) {
        currentPosition += elementsCount;
        targetPageValue += elementsCount;
      }

      var clampedPivot = Mathf.Clamp(pivotState + posDiff,
        -selectionWindowWidth / 2 / SpaceBetweenSelectedAndAdjacentPages,
        selectionWindowWidth / 2 / SpaceBetweenSelectedAndAdjacentPages);
      pivotState = clampedPivot;


      float calcDeltaPosAbs(int idx, float elementPos) => Mathf.Abs(idx - elementPos + pivotState);

      for (var idx = 0; idx < elements.Count; idx++) {
        var elementPos = currentPosition;
        if (loopable) {
          var best = calcDeltaPosAbs(idx, elementPos);
          void findBestElementPos(float newElementPos) {
            var cur = calcDeltaPosAbs(idx, newElementPos);
            if (cur < best) {
              best = cur;
              elementPos = newElementPos;
            }
          }

          findBestElementPos(currentPosition - elements.Count);
          findBestElementPos(currentPosition + elements.Count);
        }

        var indexDiff = Mathf.Abs(idx - elementPos);
        var sign = Mathf.Sign(idx - elementPos);
        var deltaPos =
          Mathf.Clamp01(indexDiff) * SpaceBetweenSelectedAndAdjacentPages +
          Mathf.Max(0, indexDiff - 1) * SpaceBetweenOtherPages;

        foreach (var distance in disableDistantElements) {
          elements[idx].gameObject.SetActive(deltaPos < distance);
        }

        var t = elements[idx].gameObject.transform;

        var pivot = (loopable || whenNotLoopableMove) ? pivotState : -(elementsCount - 1) / 2f + currentPosition;

        t.localPosition = getPosition(_direction, deltaPos * sign, indexDiff, selectedPageOffset, pivot * SpaceBetweenSelectedAndAdjacentPages);

        t.localScale = Vector3.one * (indexDiff < 1
          ? Mathf.Lerp(SelectedPageItemsScale, OtherPagesItemsScale, indexDiff)
          :
            maxElementsFromCenter.value.fold(
              () => Mathf.Lerp(OtherPagesItemsScale, AdjacentToSelectedPageItemsScale, indexDiff - 1),
              maxElementsFromCenter => Mathf.Lerp(
                AdjacentToSelectedPageItemsScale, 0, indexDiff - maxElementsFromCenter
              )
            )
        );
      }
    }

    static Vector3 getPosition(
      Carousel.Direction carouselDirection, float distanceFromPivot, float absDiff,
      Option<Vector3> centralItemOffset, float pivotPos
    ) {
      var newPos = carouselDirection == Carousel.Direction.Horizontal
        ? new Vector3(distanceFromPivot + pivotPos, 0, 0)
        : new Vector3(0, -distanceFromPivot - pivotPos, 0);

      foreach (var offset in centralItemOffset) {
        var lerpedOffset = Vector3.Lerp(offset, Vector3.zero, absDiff);
        return newPos + lerpedOffset;
      }

      return newPos;
    }

    /// <summary>
    /// Immediately refresh carousel content. Call after modifying <see cref="elements"/> to prevent
    /// visual flicker.
    /// </summary>
    public void forceUpdate() => lerpPosition(1);

    public void handleCarouselSwipe(SwipeDirection swipeDirection) {
      switch (_direction) {
        case Carousel.Direction.Horizontal:
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
        case Carousel.Direction.Vertical:
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
          throw new ArgumentOutOfRangeException(nameof(_direction), _direction, null);
      }
    }

    public void OnDrawGizmosSelected() {
      Gizmos.color = Color.blue;
      var halfOfSelectionWindow = selectionWindowWidth / 2;
      var lineLength =
        _direction == Carousel.Direction.Horizontal
          ? ((RectTransform)transform).rect.height
          : ((RectTransform)transform).rect.width;
      var halfLineLength = lineLength / 2;

      Vector3 t(Vector3 v) => transform.TransformPoint(v);

      switch (_direction) {
        case Carousel.Direction.Horizontal:
          Gizmos.DrawLine(t(new Vector3(-halfOfSelectionWindow, -halfLineLength)), t(new Vector3(-halfOfSelectionWindow, halfLineLength)));
          Gizmos.DrawLine(t(new Vector3(halfOfSelectionWindow, -halfLineLength)), t(new Vector3(halfOfSelectionWindow, halfLineLength)));
          break;
        case Carousel.Direction.Vertical:
          Gizmos.DrawLine(t(new Vector3(-halfLineLength, -halfOfSelectionWindow)), t(new Vector3(halfLineLength, -halfOfSelectionWindow)));
          Gizmos.DrawLine(t(new Vector3(-halfLineLength, halfOfSelectionWindow)), t(new Vector3(halfLineLength, halfOfSelectionWindow)));
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_direction), _direction, null);
      }
    }
  }

  public struct CarouselGameObject : ICarouselItem {
    public GameObject gameObject { get; }

    public CarouselGameObject(GameObject gameObject) {
      this.gameObject = gameObject;
    }

    public static implicit operator CarouselGameObject(GameObject o) => new CarouselGameObject(o);
  }
}