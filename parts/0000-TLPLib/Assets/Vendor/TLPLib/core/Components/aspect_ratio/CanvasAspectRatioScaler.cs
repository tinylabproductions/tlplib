
using com.tinylabproductions.TLPLib.Logger;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.aspect_ratio {
  [
    ExecuteAlways,
    // Help(
    //   HelpType.Info, HelpPosition.Before,
    //   "This script will changes local scale of this game object to account " +
    //   "for the changes of screen size.\n\n" +
    //   "This script should be used for elements which are in Canvas. For non-canvas elements use " +
    //   nameof(ScreenAspectRatioScaler) + " script.\n\n" +
    //   "!!! This component needs to be on a game object which is set to 'Stretch over all parent' " +
    //   "on RectTransform and left, top, right, bottom should = 0 !!!"
    // )
  ]
  public class CanvasAspectRatioScaler : UIBehaviour, ILayoutController {
    enum StretchMode { None, Horizontal, Vertical }

    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [
      SerializeField,
      // Help(
      //   HelpType.Info,
      //   "Target game object to scale. Must NOT be stretched across parent!"
      // )
    ] protected RectTransform target;
    [SerializeField] float originalWidth, originalHeight;
    [SerializeField] StretchMode stretchMode;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    RectTransform _transform;
    DrivenRectTransformTracker rtTracker = new DrivenRectTransformTracker();

    // transform may be null if OnRectTransformDimensionsChange was called before Awake
    new RectTransform transform => _transform ? _transform : (RectTransform)base.transform;

    public float scale { get; private set; }

    [ShowInInspector]
    float calculatedScale {
      get {
        var tr = transform;
        var widthScaleRatio = originalWidth / tr.rect.width;
        var heightScaleRatio = originalHeight / tr.rect.height;
        return 1 / Mathf.Max(widthScaleRatio, heightScaleRatio);
      }
    }

    protected override void Awake() {
      _transform = (RectTransform) base.transform;
      if (_transform == target && Log.d.isWarn())
        Log.d.warn($"{nameof(target)} == self on {this}!");
    }

    // We force to recalculate sizes on Start() because Unity is not persistent and there are cases when it
    // resizes objects after the last OnRectTransformDimensionsChange() call.
    protected override void Start() {
      base.Start();
      OnRectTransformDimensionsChange();
    }

    protected override void OnEnable() => OnRectTransformDimensionsChange();
    protected override void OnDisable() => rtTracker.Clear();

    protected override void OnRectTransformDimensionsChange() {
      if (!IsActive()) return;

      var tr = transform;
      scale = calculatedScale;
      target.localScale = Vector3.one * scale;

      var properties = DrivenTransformProperties.Scale;

      if (stretchMode != StretchMode.None) {
        properties |= stretchMode == StretchMode.Vertical
          ? DrivenTransformProperties.SizeDeltaY
          : DrivenTransformProperties.SizeDeltaX;
        var axis = stretchMode == StretchMode.Vertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
        var size = axis == RectTransform.Axis.Vertical ? tr.rect.height : tr.rect.width;
        target.SetSizeWithCurrentAnchors(axis, size / scale);
      }

      rtTracker.Clear();
      rtTracker.Add(this, target, properties);
    }

    public void SetLayoutHorizontal() => OnRectTransformDimensionsChange();

    public void SetLayoutVertical() => OnRectTransformDimensionsChange();
  }
}