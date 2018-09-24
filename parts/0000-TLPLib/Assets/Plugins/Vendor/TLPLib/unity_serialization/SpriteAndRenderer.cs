using System;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  [Serializable] public partial struct SpriteAndRenderer {
    [SerializeField, NotNull, PublicAccessor] Sprite _sprite;
    [SerializeField, NotNull, PublicAccessor] SpriteRenderer _renderer;
  }
  [Serializable] public class SpriteAndRendererOption : UnityOption<SpriteAndRenderer> {}
  [Serializable] public class SpritesAndRenderersOption : UnityOption<SpriteAndRenderer[]> {}
  
}