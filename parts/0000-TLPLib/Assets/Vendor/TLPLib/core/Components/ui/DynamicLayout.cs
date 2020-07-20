using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.ui {
  /// <summary>
  /// Scrollable vertical/horizontal layout, which makes sure that only visible elements are created.
  /// Element is considered visible if it intersects with <see cref="_maskRect"/> bounds.
  ///
  /// Sample vertical layout:
  /// 
  /// <code><![CDATA[
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
  /// ]]></code>
  /// </summary>
  public partial class DynamicLayout : MonoBehaviour {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull, PublicAccessor] ScrollRect _scrollRect;
    [SerializeField, NotNull] RectTransform _container;
    [SerializeField, NotNull, PublicAccessor] RectTransform _maskRect;
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
    public interface IElementData {
      /// <summary>Height of an element in a vertical layout OR width in horizontal layout</summary>
      float sizeInScrollableAxis { get; }
      /// <summary>Item width portion of vertical layout width OR height in horizontal layout.</summary>
      Percentage sizeInSecondaryAxis { get; }
      Option<IElementWithViewData> asElementWithView { get; }
    }
    
    public interface IElementWithViewData : IElementData {
      /// <summary>
      /// Function to create a layout item.
      /// It is expected that you take <see cref="IElementView"/> from a pool when <see cref="createItem"/> is called
      /// and release an item to the pool on <see cref="IDisposable.Dispose"/>
      /// </summary>
      IElementView createItem(Transform parent);
    }

    /// <summary>
    /// Empty spacer element
    /// </summary>
    public class EmptyElement : IElementData {
      public float sizeInScrollableAxis { get; }
      public Percentage sizeInSecondaryAxis { get; }
      public Option<IElementWithViewData> asElementWithView => None._;

      public static EmptyElement createVertical(float height, Percentage width) => new EmptyElement(
        sizeInScrollableAxis: height,
        sizeInSecondaryAxis: width
      );
      public static EmptyElement createHorizontal(float width, Percentage height) => new EmptyElement(
        sizeInScrollableAxis: width,
        sizeInSecondaryAxis: height
      );
      
      EmptyElement(float sizeInScrollableAxis, Percentage sizeInSecondaryAxis) {
        this.sizeInScrollableAxis = sizeInScrollableAxis;
        this.sizeInSecondaryAxis = sizeInSecondaryAxis;
      }
    }
    
    public class Init : IDisposable {
      const float EPS = 1e-9f;

      readonly DisposableTracker dt = new DisposableTracker();
      readonly DynamicLayout backing;
      readonly List<IElementData> layoutData;
      readonly IRxRef<float> containerSizeInScrollableAxis = RxRef.a(0f);
      readonly bool renderLatestItemsFirst;
      readonly bool isHorizontal;

      // If Option is None, that means there is no backing view, it is only a spacer.
      readonly IDictionary<IElementData, Option<IElementView>> _items = 
        new Dictionary<IElementData, Option<IElementView>>();

      public Option<Option<IElementView>> get(IElementData key) => _items.get(key);

      public Init(
        DynamicLayout backing,
        IEnumerable<IElementData> layoutData,
        bool renderLatestItemsFirst = false
      ) {
        this.backing = backing;
        this.layoutData = layoutData.ToList();
        this.renderLatestItemsFirst = renderLatestItemsFirst;
        isHorizontal = backing._scrollRect.horizontal;

        var mask = backing._maskRect;

        // We need oncePerFrame() because Unity doesn't allow doing operations like gameObject.SetActive()
        // from OnRectTransformDimensionsChange()
        // oncePerFrame() performs operation in LateUpdate
        var maskSize = 
          mask.gameObject.EnsureComponent<OnRectTransformDimensionsChangeForwarder>().rectDimensionsChanged
          .oncePerFrame()
          .filter(_ => mask) // mask can go away before late update, so double check it.
          .map(_ => mask.rect)
          .toRxVal(mask.rect);

        var rectTransformAxis = isHorizontal
          ? RectTransform.Axis.Horizontal
          : RectTransform.Axis.Vertical;
        maskSize.zip(containerSizeInScrollableAxis, (_, size) => size).subscribe(dt, size => {
          backing._container.SetSizeWithCurrentAnchors(rectTransformAxis, size);
          clearLayout();
          updateLayout();
        });

        backing._scrollRect.onValueChanged.subscribe(dt, _ => updateLayout());
      }

      /// <param name="element"></param>
      /// <param name="updateLayout">
      /// pass false and then call <see cref="updateLayout"/> manually when doing batch updates
      /// </param>
      [PublicAPI]
      public void appendDataIntoLayoutData(IElementData element, bool updateLayout = true) {       
        layoutData.Add(element);
        if (updateLayout) this.updateLayout();
      }

      [PublicAPI]
      public void clearLayoutData() {
        layoutData.Clear();
        updateLayout();
      }
      
      void clearLayout() {
        foreach (var kv in _items) {
          foreach (var item in kv.Value) item.Dispose();
        }
        _items.Clear();
      }
      
      public Rect calculateVisibleRect => backing._maskRect.rect.convertCoordinateSystem(
        ((Transform)backing._maskRect).some(), backing._container
      );

      /// <summary>
      /// You can call this after modifying the underlying data to update the layout so
      /// it would show everything correctly.
      /// </summary>
      [PublicAPI]
      public void updateLayout() {
        var visibleRect = calculateVisibleRect;

        var totalOffsetUntilThisRow = 0f;
        var currentRowSizeInScrollableAxis = 0f;
        var currentSizeInSecondaryAxisPerc = 0f;

        var direction = renderLatestItemsFirst ? -1 : 1;
        for (
          var idx = renderLatestItemsFirst ? layoutData.Count - 1 : 0;
          renderLatestItemsFirst ? idx >= 0 : idx < layoutData.Count;
          idx += direction
        ) {
          var data = layoutData[idx];
          var itemSizeInSecondaryAxisPerc = data.sizeInSecondaryAxis.value;
          var itemLeftPerc = 0f;
          if (currentSizeInSecondaryAxisPerc + itemSizeInSecondaryAxisPerc > 1f + EPS) {
            currentSizeInSecondaryAxisPerc = itemSizeInSecondaryAxisPerc;
            totalOffsetUntilThisRow += currentRowSizeInScrollableAxis;
            currentRowSizeInScrollableAxis = data.sizeInScrollableAxis;
          }
          else {
            itemLeftPerc = currentSizeInSecondaryAxisPerc;
            currentSizeInSecondaryAxisPerc += itemSizeInSecondaryAxisPerc;
            currentRowSizeInScrollableAxis = Mathf.Max(currentRowSizeInScrollableAxis, data.sizeInScrollableAxis);
          }

          Rect cellRect;
          if (isHorizontal) {
            var height = backing._container.rect.height;
            var y = itemLeftPerc * height;
            cellRect = new Rect(
              x: totalOffsetUntilThisRow,
              y: y,
              width: data.sizeInScrollableAxis,
              height: height * itemSizeInSecondaryAxisPerc
            );
          }
          else {
            var width = backing._container.rect.width;
            var x = itemLeftPerc * width;
            cellRect = new Rect(
              x: x,
              y: -totalOffsetUntilThisRow - data.sizeInScrollableAxis,
              width: width * itemSizeInSecondaryAxisPerc,
              height: data.sizeInScrollableAxis
            );            
          }

             
          var placementVisible = visibleRect.Overlaps(cellRect, true);
          
          if (placementVisible && !_items.ContainsKey(data)) {
            var instanceOpt = Option<IElementView>.None;
            foreach (var elementWithView in data.asElementWithView) {
              var instance = elementWithView.createItem(backing._container);
              var rectTrans = instance.rectTransform;
              rectTrans.anchorMin = rectTrans.anchorMax = isHorizontal ? Vector2.zero : Vector2.up;
              rectTrans.localPosition = Vector3.zero;
              rectTrans.anchoredPosition = cellRect.center;
              instanceOpt = instance.some();
            }
            _items.Add(data, instanceOpt);
          }
          else if (!placementVisible && _items.ContainsKey(data)) {
            var itemOpt = _items[data];
            _items.Remove(data);
            foreach (var item in itemOpt) {
              item.Dispose();
            }
          }
        }
        containerSizeInScrollableAxis.value = totalOffsetUntilThisRow + currentRowSizeInScrollableAxis;
      }

      public void Dispose() {
        clearLayout();
        dt.Dispose();
      }
    }
  }
}