using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public class GradientTextureSpriteRenderer : GradientTextureBase {
#pragma warning disable 649
    // ReSharper disable once NotNullMemberIsNotInitialized
    [SerializeField, NotNull] SpriteRenderer spriteRenderer;
#pragma warning restore 649

    protected override void setTexture(Texture2D texture) => spriteRenderer.sprite = texture.toSprite();
  }
}
