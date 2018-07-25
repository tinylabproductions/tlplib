using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public class GradientTextureSpriteRenderer : GradientTextureBase {
    // ReSharper disable once NotNullMemberIsNotInitialized
    [SerializeField, NotNull] SpriteRenderer spriteRenderer;

    protected override void setTexture(Texture2D texture) => spriteRenderer.sprite = texture.toSprite();
  }
}
