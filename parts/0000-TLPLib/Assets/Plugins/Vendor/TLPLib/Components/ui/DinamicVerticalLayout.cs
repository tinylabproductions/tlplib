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
  public class DinamicVerticalLayout : MonoBehaviour {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] ScrollRect _scrollRect;
    [SerializeField, NotNull] RectTransform _container, _maskRect;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    public OnRectTransformDimensionsChangeForwarder container => _container.gameObject.EnsureComponent<OnRectTransformDimensionsChangeForwarder>();
    public OnRectTransformDimensionsChangeForwarder mask => _maskRect.gameObject.EnsureComponent<OnRectTransformDimensionsChangeForwarder>();

    public interface ILayoutItem : IDisposable {
      RectTransform rectTransform { get; }
    }

    public interface IData {
      float height { get; }
      Percentage width { get; }
      ILayoutItem createItem(Transform parent);
    }

    public class Init : IDisposable {
      const float EPS = 1e-9f;

      readonly DisposableTracker dt = new DisposableTracker();
      readonly DinamicVerticalLayout backing;
      readonly ImmutableArray<IData> layoutData;
      readonly IRxVal<Rect> maskSize;
      readonly IRxRef<float> containerHeight = RxRef.a(0f);
      readonly Dictionary<IData, ILayoutItem> items = new Dictionary<IData, ILayoutItem>();

      public Init(
        DinamicVerticalLayout backing, 
        ImmutableArray<IData> layoutData
      ) {
        this.backing = backing;
        this.layoutData = layoutData;

        // We need oncePerFrame() because Unity doesn't allow doing operations like gameObject.SetActive() 
        // from OnRectTransformDimensionsChange()
        var mask = backing.mask;
        maskSize = 
          mask.rectDimensionsChanged.oncePerFrame().map(_ => mask.rectTransform.rect).toRxVal(mask.rectTransform.rect);

        dt.track(maskSize.zip(containerHeight).subscribe(tpl => {
          var height = tpl._2;
          backing.container.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
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
        var visibleRect = backing.mask.rectTransform.rect.convertCoordinateSystem(
          backing.mask.transform.some(), backing.container.rectTransform
        );

        var height = 0f;
        var currentWidthPerc = 0f;
        foreach (var data in layoutData) {
          var itemWidthPerc = data.width.value;
          var itemLeftPerc = 0f;
          if (currentWidthPerc + itemWidthPerc > 1f + EPS) {
            currentWidthPerc = itemWidthPerc;
          }
          else {
            itemLeftPerc = currentWidthPerc;
            currentWidthPerc += itemWidthPerc;
          }

          if (Mathf.Approximately(itemLeftPerc, 0f)) height += data.height;

          var x = itemLeftPerc * maskSize.value.width;
          var cellRect = Rect.MinMaxRect(x, -height, x + maskSize.value.width * itemWidthPerc, -height + data.height) ;
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
        containerHeight.value = height;
      }

      public void Dispose() {
        clearLayout();
        dt.Dispose();
      }
    }
  }
}