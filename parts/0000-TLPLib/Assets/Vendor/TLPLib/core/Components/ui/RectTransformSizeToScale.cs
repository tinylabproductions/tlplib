
using com.tinylabproductions.TLPLib.Logger;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.ui {
  public class RectTransformSizeToScale : UIBehaviour, ILayoutController {
#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [SerializeField] Vector2 _scaleRatio = Vector2.one;
    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 649
    
    protected override void Awake() => init();
    
    RectTransform rt;
    DrivenRectTransformTracker rtTracker = new DrivenRectTransformTracker();

    void init() {
      if (rt) return;
      rt = GetComponent<RectTransform>();
      rtTracker.Clear();
      rtTracker.Add(this, rt, DrivenTransformProperties.Scale);
    }
    
    protected override void OnRectTransformDimensionsChange() => selfLayout();

    protected override void OnEnable() => selfLayout();
    protected override void OnDisable() {
      rtTracker.Clear();
      rt = null;
    }

    void selfLayout() {
      if (!IsActive()) return;
      
      init();

      var size = rt.rect.size;
      rt.localScale = new Vector3(size.x * _scaleRatio.x, size.y * _scaleRatio.y, 1);
    }

    public void SetLayoutHorizontal() => selfLayout();

    public void SetLayoutVertical() => selfLayout();
    
#if UNITY_EDITOR
    protected override void OnValidate() {
      selfLayout();
    }
#endif
  }
}