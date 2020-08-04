using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vendor.TLPLib.Components.ui {
  public class ResizeToSafeAreaOffsets : UIBehaviour, IMB_Update {
#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] RectTransform _rt;
    [SerializeField, NotNull] List<RectTransform> _negativeOffsetLeft, _negativeOffsetRight, _negativeOffsetAll, _negativeOffsetBottom;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    RectTransform parent;
    Rect lastSafeArea = new Rect(0, 0, 0, 0);
    bool forceRefresh;

#pragma warning disable 649
    [ShowInInspector] float __editor_leftOffsetTest, __editor_rightOffsetTest, __editor_bottomOffsetTest;
#pragma warning restore 649

    protected override void Awake() {
      parent = (RectTransform) _rt.parent;
      refresh();
    }

    public void Update() => refresh();

    public void addToNegativeOffsetLeft(RectTransform t) {
      _negativeOffsetLeft.Add(t);
      forceRefresh = true;
    }

    public void addToNegativeOffsetRight(RectTransform t) {
      _negativeOffsetRight.Add(t);
      forceRefresh = true;
    }

    public void addToNegativeOffsetAll(RectTransform t) {
      _negativeOffsetAll.Add(t);
      forceRefresh = true;
    }

    public void addToNegativeOffsetBottom(RectTransform t) {
      _negativeOffsetBottom.Add(t);
      forceRefresh = true;
    }

    void refresh() {
      var safeArea = Screen.safeArea;
      if (Application.isEditor) {
        safeArea.xMin += __editor_leftOffsetTest;
        safeArea.xMax -= __editor_rightOffsetTest;
        safeArea.yMin += __editor_bottomOffsetTest;
      }
      if (forceRefresh || safeArea != lastSafeArea) {
        forceRefresh = false;
        lastSafeArea = safeArea;
        applySafeArea(safeArea, new Vector2(Screen.width, Screen.height));
      }
    }

    void applySafeArea(Rect safeArea, Vector2 screenSize) {
      var scale = parent.sizeDelta / screenSize;

      var min = safeArea.min * scale;
      var max = (screenSize - safeArea.max) * scale;

      _rt.offsetMin = min;
      // offsetMax is inverted in unity
      _rt.offsetMax = -max;

      foreach (var item in _negativeOffsetLeft) {
        var offset = item.offsetMin;
        offset.x = -min.x;
        item.offsetMin = offset;
      }

      foreach (var item in _negativeOffsetRight) {
        var offset = item.offsetMax;
        offset.x = max.x;
        item.offsetMax = offset;
      }

      foreach (var item in _negativeOffsetAll) {
        item.offsetMin = -min;
        item.offsetMax = max;
      }

      foreach (var item in _negativeOffsetBottom) {
        var offset = item.offsetMin;
        offset.y = -min.y;
        item.offsetMin = offset;
      }
    }
  }
}