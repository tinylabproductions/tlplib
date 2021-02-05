using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Pools;
using com.tinylabproductions.TLPLib.Reactive;using pzd.lib.reactive;

using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.dispose;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.EventSystems;
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
  public partial class DynamicLayout : UIBehaviour, IMB_OnEnable {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull, PublicAccessor] ScrollRect _scrollRect;
    [SerializeField, NotNull] RectTransform _container;
    [SerializeField, NotNull, PublicAccessor] RectTransform _maskRect;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    event Action onEnable;
    public void OnEnable() => onEnable?.Invoke();

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
      
      public EmptyElement(float sizeInScrollableAxis, Percentage sizeInSecondaryAxis) {
        this.sizeInScrollableAxis = sizeInScrollableAxis;
        this.sizeInSecondaryAxis = sizeInSecondaryAxis;
      }
    }
    
    public class Init {
      const float EPS = 1e-9f;

      readonly RectTransform _container, _maskRect;
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
        IDisposableTracker dt,
        bool renderLatestItemsFirst = false
      ) : this(
        backing._container, backing._maskRect, layoutData, backing._scrollRect.horizontal,
        dt, renderLatestItemsFirst
      ) {
        backing._scrollRect.onValueChanged.subscribe(dt, _ => updateLayout());
        // When we add elements to layout and enable it on the same frame,
        // layout does not work correctly due to rect sizes == 0.
        // Unable to solve this properly. NextFrame is a workaround. 
        backing.onEnable += () => ASync.NextFrame(backing.gameObject, updateLayout);
      }

      public Init(
        RectTransform _container, RectTransform _maskRect,
        IEnumerable<IElementData> layoutData,
        bool isHorizontal,
        IDisposableTracker dt,
        bool renderLatestItemsFirst = false
      ) {
        this._container = _container;
        this._maskRect = _maskRect;
        this.layoutData = layoutData.ToList();
        this.isHorizontal = isHorizontal;
        this.renderLatestItemsFirst = renderLatestItemsFirst;

        dt.track(clearLayout);

        var mask = _maskRect;

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
          _container.SetSizeWithCurrentAnchors(rectTransformAxis, size);
          clearLayout();
          updateLayout();
        });

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
      
      public Rect calculateVisibleRect => _maskRect.rect.convertCoordinateSystem(
        ((Transform) _maskRect).some(), _container
      );

      /// <summary>
      /// You can call this after modifying the underlying data to update the layout so
      /// it would show everything correctly.
      /// </summary>
      [PublicAPI]
      public void updateLayout() {
        var visibleRect = calculateVisibleRect;
        var containerRect = _container.rect;
        var containerHeight = containerRect.height;
        var containerWidth = containerRect.width;
        
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
            var yPos = itemLeftPerc * containerHeight;
            var itemHeight = containerHeight * itemSizeInSecondaryAxisPerc;
            
            // NOTE: y axis goes up, but we want to place the items from top to bottom
            // Y = 0                  ------------------------------
            //                        |                            |
            // Y = -yPos              | ---------                  |
            //                        | |       |                  |
            //                        | | item  |     Container    |
            //                        | |       |                  |
            // Y = -yPos - itemHeight | ---------                  |
            //                        |                            |
            // Y = -containerHeight   |-----------------------------
            
            // cellRect pivot point (x,y) is at bottom left of the item
            cellRect = new Rect(
              x: totalOffsetUntilThisRow,
              y: -yPos - itemHeight,
              width: data.sizeInScrollableAxis,
              height: itemHeight
            );
          }
          else {
            var x = itemLeftPerc * containerWidth;
            cellRect = new Rect(
              x: x,
              y: -totalOffsetUntilThisRow - data.sizeInScrollableAxis,
              width: containerWidth * itemSizeInSecondaryAxisPerc,
              height: data.sizeInScrollableAxis
            );            
          }
             
          var placementVisible = visibleRect.Overlaps(cellRect, true);
          
          if (placementVisible && !_items.ContainsKey(data)) {
            var instanceOpt = Option<IElementView>.None;
            foreach (var elementWithView in data.asElementWithView) {
              var instance = elementWithView.createItem(_container);
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
        containerSizeInScrollableAxis.value = totalOffsetUntilThisRow + currentRowSizeInScrollableAxis;
      }
    }

    public abstract class ElementWithViewData<Obj> : IElementWithViewData where Obj : MonoBehaviour {
      readonly GameObjectPool<Obj> pool;
      public float sizeInScrollableAxis { get; }
      public Percentage sizeInSecondaryAxis { get; }
      
      public Option<IElementWithViewData> asElementWithView => F.some<IElementWithViewData>(this);
      
      protected abstract IDisposable setup(Obj view);

      public ElementWithViewData(
        GameObjectPool<Obj> pool, float sizeInScrollableAxis, Percentage sizeInSecondaryAxis
      ) {
        this.pool = pool;
        this.sizeInSecondaryAxis = sizeInSecondaryAxis;
        this.sizeInScrollableAxis = sizeInScrollableAxis;
      }

      public IElementView createItem(Transform parent) {
        var view = pool.borrow();
        return new ElementView<Obj>(view, setup(view), pool);
      }
    }
    
    public class ElementView<Obj> : IElementView where Obj : MonoBehaviour {
      readonly Obj visual;
      readonly IDisposable disposable;
      readonly GameObjectPool<Obj> pool;
      public RectTransform rectTransform { get; }
      
      public ElementView(Obj visual, IDisposable disposable, GameObjectPool<Obj> pool) {
        this.visual = visual;
        this.disposable = disposable;
        this.pool = pool;
        rectTransform = (RectTransform) visual.transform;
      }
      public void Dispose() {
        if (visual) pool.release(visual);
        disposable.Dispose();
      }
    }
  }
}