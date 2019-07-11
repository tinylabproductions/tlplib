using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vendor.TLPLib.Components.ui {
  public class ResizeToSafeAreaOffsets : UIBehaviour, IMB_Update {

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] RectTransform _rt;
    [SerializeField, NotNull] RectTransform[] _negativeOffsetLeft, _negativeOffsetRight, _negativeOffsetAll;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    RectTransform parent;
    Rect lastSafeArea = new Rect(0, 0, 0, 0);

    protected override void Awake() {
      parent = (RectTransform) _rt.parent;
      refresh();
    }

    public void Update () => refresh();

    void refresh() {
      var safeArea = Screen.safeArea;
      if (safeArea != lastSafeArea) {
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
    }
  }
}