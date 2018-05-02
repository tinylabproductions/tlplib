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
    public bool wrapCarouselAround;
    bool wrapableAround => wrapCarouselAround;
    [
      SerializeField,
      Tooltip(
        "If wraparound is enabled, how many elements do we have to have in the carousel " +
        "to start wrapping around? This is needed, because if, for example, we only have 2 " +
        "elements and they fit into the screen, user can see the wraparound moving the elements " +
        "in the view."
      ),
      Inspect(nameof(wrapableAround))
    ] int _minElementsForWraparound = 5;
    [SerializeField] UnityOptionInt maxElementsFromCenter;
    [SerializeField] UnityOptionVector3 selectedPageOffset;
    // ReSharper disable once NotNullMemberIsNotInitialized
    [SerializeField] Carousel.Direction _direction = Carousel.Direction.Horizontal;
    // FIXME: There's still a visual issue when all items fits into selection window
    // for example when selection window width is 500 and you have 3 elements of width 100. 
    [
      SerializeField, 
      Tooltip(
        "Width of a window for selection from the center of a carousel.\n" +
        "\n" +
        "When a new element is selected, its center will be moved so that it is within the " +
        "selection window. For example, if the window width is 0 then the selected element will " +
        "always be centered. If it will be 100, the selected element center point will always be " +
        "between x [-50; 50]."
      )
    ] float selectionWindowWidth;

    protected override void OnValidate() {
      selectionWindowWidth = Math.Max(selectionWindowWidth, 0);
    }
#pragma warning restore 649

    #endregion

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
    bool loopable => wrapCarouselAround && elements.Count >= _minElementsForWraparound;

    readonly RxRef<int> _page = new RxRef<int>(0);
    public IRxVal<int> page => _page;

    readonly LazyVal<IRxRef<Option<A>>> __currentElement;
    public IRxVal<Option<A>> currentElement => __currentElement.strict;

    public readonly IRxRef<bool> freezeCarouselMovement = RxRef.a(false);

    void updateCurrentElement() {
      if (__currentElement.isCompleted) {
        __currentElement.strict.value = elements.get(_page.value);
      }
    }

    /// Page position between previously selected page index and <see cref="targetPageValue"/>
    float currentPosition;

    float pivotFromCenter;

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
    public void setPageAnimated(int targetPage) {
      if (elements.isEmpty()) return;
      var current = targetPage - _page.value;

      // Searches for shortest travel distance towards targetPage
      void findBestPage(int p) {
        if (Math.Abs(p + pivotFromCenter) <= Math.Abs(current + pivotFromCenter)) {
          current = p;
        }
      }
      findBestPage(targetPage - _page.value - elementsCount);
      findBestPage(targetPage - _page.value + elementsCount);
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

      // Position is kept between 0 and elementsCount to
      // prevent scrolling multiple times if targetPageValue is something like 100 but we only have 5 elements
      {
        while (currentPosition > elementsCount) {
          currentPosition -= elementsCount;
          targetPageValue -= elementsCount;
        }

        while (currentPosition < 0) {
          currentPosition += elementsCount;
          targetPageValue += elementsCount;
        }
      }

      var itemCountFittingToWindow = selectionWindowWidth / SpaceBetweenOtherPages;
      pivotFromCenter = Mathf.Clamp(pivotFromCenter + posDiff, -itemCountFittingToWindow / 2, itemCountFittingToWindow / 2);
      var pivot = freezeCarouselMovement.value ? -(elementsCount - 1) / 2f + currentPosition : pivotFromCenter;

      float calcDeltaPosAbs(int idx, float elementPos) => Mathf.Abs(idx - elementPos + pivotFromCenter);

      for (var idx = 0; idx < elements.Count; idx++) {
        var elementPos = currentPosition;

        // Calculate element's position closest to pivot
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

        t.localPosition =
          getPosition(
            _direction,
            deltaPos * sign,
            indexDiff,
            selectedPageOffset,
            pivot * SpaceBetweenSelectedAndAdjacentPages
          );

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
      var rectTransform = (RectTransform) transform;
      var lineLength =
        _direction == Carousel.Direction.Horizontal
          ? rectTransform.rect.height
          : rectTransform.rect.width;
      var halfOfSelectionWindow = selectionWindowWidth / 2;
      var halfLineLength = lineLength / 2;

      Vector3 t(Vector3 v) => transform.TransformPoint(v);

      switch (_direction) {
        case Carousel.Direction.Horizontal:
          Gizmos.DrawLine(
            t(new Vector3(-halfOfSelectionWindow, -halfLineLength)), 
            t(new Vector3(-halfOfSelectionWindow, halfLineLength))
          );
          Gizmos.DrawLine(
            t(new Vector3(halfOfSelectionWindow, -halfLineLength)), 
            t(new Vector3(halfOfSelectionWindow, halfLineLength))
          );
          break;
        case Carousel.Direction.Vertical:
          Gizmos.DrawLine(
            t(new Vector3(-halfLineLength, -halfOfSelectionWindow)), 
            t(new Vector3(halfLineLength, -halfOfSelectionWindow))
          );
          Gizmos.DrawLine(
            t(new Vector3(-halfLineLength, halfOfSelectionWindow)), 
            t(new Vector3(halfLineLength, halfOfSelectionWindow))
          );
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