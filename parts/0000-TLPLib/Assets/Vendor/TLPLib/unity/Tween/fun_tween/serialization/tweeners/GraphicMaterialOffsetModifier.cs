using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  /// <summary>
  ///
  /// </summary>
  [RequireComponent(typeof(Graphic)), ExecuteInEditMode]
  public class GraphicMaterialOffsetModifier : MonoBehaviour, IMaterialModifier, IMB_LateUpdate {
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized

    // Serialized offset is used for 2 reasons:
    // 1. You can change offset in edit mode by changing this value.
    // 2. So it would undo the value to the default one after testing tween in edit mode.
    [SerializeField] Vector2 _offset;

    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649

    Material material;

    public Vector2 offset {
      get => _offset;
      set => _offset = value;
    }

    public void LateUpdate() {
      if (material) material.mainTextureOffset = _offset;
    }

    public Material GetModifiedMaterial(Material baseMaterial) {
      var copy = new Material(baseMaterial) {mainTextureOffset = _offset};
      material = copy;
      return material;
    }
  }
}