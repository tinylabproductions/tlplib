using com.tinylabproductions.TLPLib.Components.EditorTools;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.aspect_ratio {
  [ExecuteInEditMode]
  public sealed class ScreenAspectRatioScaler : MonoBehaviour, IMB_Update {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [
      SerializeField,
      HelpLabel("Original aspect ratio that this object is designed for.", HelpBoxMessageType.Info),
      HelpLabel(
        "This script will change local scale of this game object to account " +
        "for the changes of screen size.\n\n" +
        "This script should be used for elements which are not in Canvas. For canvas elements use " +
        nameof(CanvasAspectRatioScaler) + " script"
      )
    ] Vector2 originalAspectRatio = new Vector2(16, 9);
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    Vector2 lastKnownScreenSize;

    public void Update() {
      var screenSize = new Vector2(Screen.width, Screen.height);
      if (lastKnownScreenSize == screenSize) return;
      // Can't cache transform here, because Unity refuses to call Awake in edit mode
      transform.localScale = calculateLocalScale(originalAspectRatio, screenSize);
      lastKnownScreenSize = screenSize;
    }

    public static Vector3 calculateLocalScale(
      Vector2 original, Vector2 current
    ) {
      var r1 = original.x / original.y;
      var r2 = current.x / current.y;
      var scale = Mathf.Min(1, r2 / r1);
      return Vector3.one * scale;
    }
  }
}