using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.ui {
  /// <summary>
  /// Scrollable vertical layout, which makes sure that only visible elements are created.
  /// Element is considered visible if it intersects with <see cref="_maskRect"/> bounds.
  ///
  /// Sample layout:
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
  public partial class DynamicVerticalLayout : MonoBehaviour {
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
      /// <summary>Height of an element in a layout.</summary>
      float height { get; }
      /// <summary>Item width portion of layout width.</summary>
      Percentage width { get; }
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
      public float height { get; }
      public Percentage width { get; }
      public Option<IElementWithViewData> asElementWithView => Option<IElementWithViewData>.None;

      public EmptyElement(float height, Percentage width) {
        this.height = height;
        this.width = width;
      }
    }
    
    public class Init : IDisposable {
      const float EPS = 1e-9f;

      readonly DisposableTracker dt = new DisposableTracker();
      readonly DynamicVerticalLayout backing;
      readonly List<IElementData> layoutData;
      readonly IRxRef<float> containerHeight = RxRef.a(0f);
      readonly bool renderLatestItemsFirst;

      // If Option is None, that means there is no backing view, it is only a spacer.
      readonly IDictionary<IElementData, Option<IElementView>> _items = 
        new Dictionary<IElementData, Option<IElementView>>();

      public Option<Option<IElementView>> get(IElementData key) => _items.get(key);

      public Init(
        DynamicVerticalLayout backing,
        IEnumerable<IElementData> layoutData,
        bool renderLatestItemsFirst = false
      ) {
        this.backing = backing;
        this.layoutData = layoutData.ToList();
        this.renderLatestItemsFirst = renderLatestItemsFirst;
        
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

        maskSize.zip(containerHeight, (_, height) => height).subscribe(dt, height => {
          backing._container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
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

      /// <summary>
      /// You can call this after modifing the underlying data to update the layout so
      /// it would show everything correctly.
      /// </summary>
      [PublicAPI]
      public void updateLayout() {
        var visibleRect = backing._maskRect.rect.convertCoordinateSystem(
          ((Transform)backing._maskRect).some(), backing._container
        );

        var totalHeightUntilThisRow = 0f;
        var currentRowHeight = 0f;
        var currentWidthPerc = 0f;

        var direction = renderLatestItemsFirst ? -1 : 1;
        for (
          var idx = renderLatestItemsFirst ? layoutData.Count - 1 : 0;
          renderLatestItemsFirst ? idx >= 0 : idx < layoutData.Count;
          idx += direction
        ) {
          var data = layoutData[idx];
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
          
          if (placementVisible && !_items.ContainsKey(data)) {
            var instanceOpt = Option<IElementView>.None;
            foreach (var elementWithView in data.asElementWithView) {
              var instance = elementWithView.createItem(backing._container);
              var rectTrans = instance.rectTransform;
              rectTrans.anchorMin = rectTrans.anchorMax = Vector2.up;
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
        containerHeight.value = totalHeightUntilThisRow + currentRowHeight;
      }

      public void Dispose() {
        clearLayout();
        dt.Dispose();
      }
    }
  }
}