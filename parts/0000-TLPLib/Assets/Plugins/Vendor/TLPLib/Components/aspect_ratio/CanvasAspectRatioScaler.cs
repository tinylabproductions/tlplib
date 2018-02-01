using AdvancedInspector;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.aspect_ratio {
  [
    ExecuteInEditMode,
    Help(
      HelpType.Info, HelpPosition.Before,
      "This script will changes local scale of this game object to account " +
      "for the changes of screen size.\n\n" +
      "This script should be used for elements which are in Canvas. For non-canvas elements use " +
      nameof(ScreenAspectRatioScaler) + " script.\n\n" +
      "!!! This component needs to be on a game object which is set to 'Stretch over all parent' " +
      "on RectTransform and left, top, right, bottom should = 0 !!!"
    )
  ]
  public class CanvasAspectRatioScaler : UIBehaviour {
    enum StretchMode : byte { None, Horizontal, Vertical }
    
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [
      SerializeField,
      Help(
        HelpType.Info, 
        "Target game object to scale. Must NOT be stretched across parent!"
      )
    ] protected RectTransform target;
    [SerializeField] float originalWidth, originalHeight;
    [SerializeField] StretchMode stretchMode;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    new RectTransform transform;

    protected override void Awake() {
      transform = (RectTransform) base.transform;
      if (transform == target && Log.d.isWarn())
        Log.d.warn($"{nameof(target)} == self on {this}!");
    }

    // We force to recalculate sizes on Start() because Unity is not persistent and there are cases when it 
    // resizes objects after the last OnRectTransformDimensionsChange() call.
    protected override void Start() {
      base.Start();
      OnRectTransformDimensionsChange();
    }

    protected override void OnRectTransformDimensionsChange() {
      base.OnRectTransformDimensionsChange();

      // transform may be null if OnRectTransformDimensionsChange was called before Awake
      var transform = this.transform ?? ((RectTransform)base.transform);
      var widthScaleRatio = originalWidth / transform.rect.width;
      var heightScaleRatio = originalHeight / transform.rect.height;
      var scale = 1 / Mathf.Max(widthScaleRatio, heightScaleRatio);
      target.localScale = Vector3.one * scale;

      if (stretchMode != StretchMode.None) {
        var axis = stretchMode == StretchMode.Vertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
        var size = axis == RectTransform.Axis.Vertical ? transform.rect.height : transform.rect.width;
        target.SetSizeWithCurrentAnchors(axis, size / scale);
      } 
    }
  }
}