using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.aspect_ratio {
  [ExecuteAlways]
  public class CanvasAspectRatioExpander : UIBehaviour, ILayoutSelfController {

    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField] float aspectRatio = (float) 16 / 9;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    [ShowInInspector]
    string humanReadable => $"{aspectRatio * 9}:9";

    RectTransform rt, parent;
    DrivenRectTransformTracker rtTracker = new DrivenRectTransformTracker();

    protected override void Awake() => init();

    void init() {
      if (rt) return;
      rt = GetComponent<RectTransform>();
      parent = (RectTransform) rt.parent;
      rtTracker.Clear();
      rtTracker.Add(
        this,
        rt,
        DrivenTransformProperties.Anchors
        | DrivenTransformProperties.AnchoredPosition
        | DrivenTransformProperties.SizeDelta
        | DrivenTransformProperties.Scale
      );
      rt.anchorMin = Vector2.zero;
      rt.anchorMax = Vector2.one;
      rt.anchoredPosition = Vector2.zero;
    }

    protected override void Start() {
      base.Start();
      selfLayout();
    }

    protected override void OnRectTransformDimensionsChange() => selfLayout();

    protected override void OnEnable() => selfLayout();
    protected override void OnDisable() {
      rtTracker.Clear();
      rt = parent = null;
    }

    void selfLayout() {
      if (!IsActive()) return;

      init();

      var parentRect = parent.rect;
      var parentAspect = parentRect.width / parentRect.height;
      if (parentAspect > aspectRatio) {
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
      }
      else {
        var scale = aspectRatio / parentAspect;
        var targetSize = parentRect.size * scale;
        rt.sizeDelta = targetSize - parentRect.size;
        rt.localScale = new Vector3(1 / scale, 1 / scale, 1);
      }
    }

    public void SetLayoutHorizontal() => selfLayout();

    public void SetLayoutVertical() => selfLayout();
  }
}