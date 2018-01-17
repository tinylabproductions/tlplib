using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.ui {
  /// <summary>
  /// Scrollable vertical layout, which makes sure that only visible ele
  /// 
  /// Sample layout:
  /// 
  ///  #  | height | width
  ///  0    10       33%
  ///  1    30       33%
  ///  2    10       33%
  ///  3    10       50%
  ///  4    10       100%
  /// 
  /// +-----+-----+-----+
  /// |  0  |  1  |  2  |
  /// +-----|     |-----+
  ///       |     |
  /// +-----+--+--+
  /// |    3   |
  /// +--------+--------+
  /// |        4        |
  /// +-----------------+
  /// 
  /// </summary>
  public class DynamicVerticalLayout : MonoBehaviour {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] ScrollRect _scrollRect;
    [SerializeField, NotNull] RectTransform _container, _maskRect;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    /// <summary>
    /// Visual part of layout item.
    /// </summary>
    public interface IElementView : IDisposable {
      RectTransform rectTransform { get; }
    }

    /// <summary>
    /// Logical part of layout item.
    /// Used to determine layout height and item positions
    /// </summary>
    /// <param name="height">Height of an element in a layout.</param>
    /// <param name="width">Item width portion of layout width.</param>
    /// <param name="createItem">
    /// Function to create a layout item.
    /// It is expected that you take <see cref="IElementView"/> from a pool when <see cref="createItem"/> is called
    /// and release an item to the pool on <see cref="IDisposable.Dispose"/>
    /// </param>
    public interface IElementData {
      float height { get; }
      Percentage width { get; }
      IElementView createItem(Transform parent);
    }

    public class Init : IDisposable {
      const float EPS = 1e-9f;

      readonly DisposableTracker dt = new DisposableTracker();
      readonly DynamicVerticalLayout backing;
      readonly ImmutableArray<IElementData> layoutData;
      readonly IRxRef<float> containerHeight = RxRef.a(0f);
      readonly Dictionary<IElementData, IElementView> items = new Dictionary<IElementData, IElementView>();

      public Init(
        DynamicVerticalLayout backing, 
        ImmutableArray<IElementData> layoutData
      ) {
        this.backing = backing;
        this.layoutData = layoutData;
        var mask = backing._maskRect;
        
        // We need oncePerFrame() because Unity doesn't allow doing operations like gameObject.SetActive() 
        // from OnRectTransformDimensionsChange()
        // oncePerFrame() performs operation in LateUpdate
        var maskSize = mask.gameObject.EnsureComponent<OnRectTransformDimensionsChangeForwarder>().rectDimensionsChanged
          .oncePerFrame()
          .map(_ => mask.rect)
          .toRxVal(mask.rect);

        dt.track(maskSize.zip(containerHeight).subscribe(tpl => {
          var height = tpl._2;
          backing._container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
          clearLayout();
          updateLayout();
        }));
        
        dt.track(backing._scrollRect.onValueChanged.subscribe(_ => updateLayout()));
      }

      void clearLayout() {
        foreach (var kv in items) {
          kv.Value.Dispose();
        }
        items.Clear();
      }

      void updateLayout() {
        var visibleRect = backing._maskRect.rect.convertCoordinateSystem(
          ((Transform)backing._maskRect).some(), backing._container
        );

        var totalHeightUntilThisRow = 0f;
        var currentRowHeight = 0f;
        var currentWidthPerc = 0f;
        foreach (var data in layoutData) {
          var itemWidthPerc = data.width.value;
          var itemLeftPerc = 0f;
          if (currentWidthPerc + itemWidthPerc > 1f + EPS) {
            currentWidthPerc = itemWidthPerc;
            totalHeightUntilThisRow += currentRowHeight;
            currentRowHeight = data.height;
          }
          else {
            itemLeftPerc = currentWidthPerc;
            currentWidthPerc += itemWidthPerc;
            currentRowHeight = Mathf.Max(currentRowHeight, data.height);
          }

          var width = backing._container.rect.width;
          var x = itemLeftPerc * width;
          var cellRect = new Rect(
            x: x, 
            y: -totalHeightUntilThisRow - data.height, 
            width: width * itemWidthPerc, 
            height: data.height
          );
          var placementVisible = visibleRect.Overlaps(cellRect, true);

          if (placementVisible && !items.ContainsKey(data)) {
            var instance = data.createItem(backing._container);
            var rectTrans = instance.rectTransform;
            rectTrans.anchorMin = Vector2.up;
            rectTrans.anchorMax = Vector2.up;
            rectTrans.localPosition = Vector3.zero;
            rectTrans.anchoredPosition = cellRect.center;
            items.Add(data, instance);
          }
          else if (!placementVisible && items.ContainsKey(data)) {
            var item = items[data];
            items.Remove(data);
            item.Dispose();
          }
        }
        containerHeight.value = totalHeightUntilThisRow + currentRowHeight;
      }

      public void Dispose() {
        clearLayout();
        dt.Dispose();
      }
    }
  }
}