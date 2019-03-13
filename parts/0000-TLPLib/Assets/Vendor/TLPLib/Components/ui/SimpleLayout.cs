using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.ui {

  public class SimpleLayout : UIBehaviour {

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, OnValueChanged(nameof(recalculateLayout))] RectTransform.Axis _axis;
    [SerializeField, OnValueChanged(nameof(recalculateLayout))] float _marginFront, _marginBack, _spacing = 10;
    [SerializeField, OnValueChanged(nameof(recalculateLayout))] bool _resizeParent;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    public float spacing {
      get => _spacing;
      set {
        _spacing = value;
        recalculateLayout();
      }
    }

    [Button]
    public void recalculateLayout() {
      var rt = (RectTransform) transform;
      var totalItems = 0;
      for (var i = 0; i < rt.childCount; i++) {
        var child = rt.GetChild(i);
        if (!child.gameObject.activeSelf) continue;
        var childRT = (RectTransform) child.transform;
        var pos = new Vector2();
        var currentSize = _marginFront + totalItems * _spacing;
        if (_axis == RectTransform.Axis.Vertical) {
          pos.x = 0;
          pos.y = -currentSize;
        }
        else {
          pos.x = currentSize;
          pos.y = 0;
        }
        childRT.anchoredPosition = pos;
        totalItems++;
      }
      if (_resizeParent) {
        rt.SetSizeWithCurrentAnchors(
          _axis,
          _marginFront + (Mathf.Max(0, totalItems - 1)) * _spacing + _marginBack
        );
      }
    }
  }
}