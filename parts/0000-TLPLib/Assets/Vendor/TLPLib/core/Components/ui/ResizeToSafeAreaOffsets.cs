using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Data;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.ui {
  public class ResizeToSafeAreaOffsets : UIBehaviour, IMB_Update {
#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] RectTransform _rt;
    [SerializeField, NotNull] List<RectTransform> 
      _negativeOffsetLeft, _negativeOffsetRight, _negativeOffsetAll, _negativeOffsetBottom, 
      _negativeOffsetSidesWithoutNotches, _customNegativeOffsetOnSidesWithoutNotches;
    [SerializeField, NotNull] Percentage _customNegativeOffset = new Percentage(.5f);
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    RectTransform parent;
    [ShowInInspector] Rect lastSafeArea = new Rect(0, 0, 0, 0);
    [ShowInInspector] ScreenOrientation lastScreenOrientation;
    bool forceRefresh;

#pragma warning disable 649
    [ShowInInspector] float __editor_leftOffsetTest, __editor_rightOffsetTest, __editor_bottomOffsetTest;
#pragma warning restore 649

    protected override void Awake() {
      parent = (RectTransform) _rt.parent;
      forceRefresh = true;
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
      var orientation = Screen.orientation;
      if (forceRefresh || safeArea != lastSafeArea || orientation != lastScreenOrientation) {
        forceRefresh = false;
        lastSafeArea = safeArea;
        lastScreenOrientation = orientation;

        var notchLeft = false;
        var notchRight = false;
        {
          // this works only for landscape
          var halfX = Screen.width / 2;
          foreach (var cutout in Screen.cutouts) {
            if (cutout.center.x < halfX) notchLeft = true;
            else notchRight = true;
          }
        }
        applySafeArea(safeArea, new Vector2(Screen.width, Screen.height), notchLeft, notchRight);
      }
    }

    void applySafeArea(Rect safeArea, Vector2 screenSize, bool notchLeft, bool notchRight) {
      var scale = parent.sizeDelta / screenSize;

      var min = safeArea.min * scale;
      var max = (screenSize - safeArea.max) * scale;

      _rt.offsetMin = min;
      // offsetMax is inverted in unity
      _rt.offsetMax = -max;
      
      foreach (var item in _negativeOffsetSidesWithoutNotches) {
        // reset old values, otherwise all sides will get negative offsets after we rotate the screen 
        item.offsetMin = Vector2.zero;
        item.offsetMax = Vector2.zero;
        if (!notchLeft) negativeLeft(item);
        if (!notchRight) negativeRight(item);
        negativeTop(item);
        negativeBottom(item);
      }
      
      foreach (var item in _customNegativeOffsetOnSidesWithoutNotches) {
        // reset old values, otherwise all sides will get negative offsets after we rotate the screen 
        item.offsetMin = Vector2.zero;
        item.offsetMax = Vector2.zero;
        if (!notchLeft) negativeLeft(item, custom: true);
        if (!notchRight) negativeRight(item, custom: true);
        negativeTop(item);
        negativeBottom(item);
      }

      foreach (var item in _negativeOffsetLeft) {
        negativeLeft(item);
      }

      foreach (var item in _negativeOffsetRight) {
        negativeRight(item);
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

      void negativeLeft(RectTransform item, bool custom = false) {
        var offset = item.offsetMin;
        offset.x = (custom ? _customNegativeOffset.value : 1) * -min.x;
        item.offsetMin = offset;
      }
      
      void negativeRight(RectTransform item, bool custom = false) {
        var offset = item.offsetMax;
        offset.x = (custom ? _customNegativeOffset.value : 1) * max.x;
        item.offsetMax = offset;
      }
      
      void negativeTop(RectTransform item) {
        var offset = item.offsetMin;
        offset.y = -min.y;
        item.offsetMin = offset;
      }
      
      void negativeBottom(RectTransform item) {
        var offset = item.offsetMax;
        offset.y = max.y;
        item.offsetMax = offset;
      }
    }
  }
}