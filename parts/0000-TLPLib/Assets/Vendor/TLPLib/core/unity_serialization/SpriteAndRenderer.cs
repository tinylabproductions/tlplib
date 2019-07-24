using System;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  [Serializable] public partial class SpriteAndRenderer {
    #region Unity Serialized Fields
#pragma warning disable 649
    // ReSharper disable UnassignedField.Global, FieldCanBeMadeReadOnly.Global
    [SerializeField, NotNull, PublicAccessor] Sprite _sprite;
    [SerializeField, NotNull, PublicAccessor] SpriteRenderer _renderer;
    
    // ReSharper restore UnassignedField.Global, FieldCanBeMadeReadOnly.Global
#pragma warning restore 649
    #endregion
  }
  [Serializable] public class SpriteAndRendererOption : UnityOption<SpriteAndRenderer> {}
  [Serializable] public class SpritesAndRenderersOption : UnityOption<SpriteAndRenderer[]> {}
  
}