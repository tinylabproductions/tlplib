﻿using AdvancedInspector;
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
      "on RectTransform. !!!"
    )
  ]
  public class CanvasAspectRatioScaler : UIBehaviour {
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
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    new RectTransform transform;

    protected override void Awake() {
      transform = (RectTransform) base.transform;
      if (transform == target && Log.d.isWarn())
        Log.d.warn($"{nameof(target)} == self on {this}!");
    }

    protected override void OnRectTransformDimensionsChange() {
      base.OnRectTransformDimensionsChange();

      target.localScale = ScreenAspectRatioScaler.calculateLocalScale(
        new Vector2(originalWidth, originalHeight), 
        new Vector2(transform.rect.width, transform.rect.height)
      );
    }
  }
}