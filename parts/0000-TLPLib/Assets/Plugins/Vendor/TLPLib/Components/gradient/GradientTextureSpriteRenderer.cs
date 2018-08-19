using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public class GradientTextureSpriteRenderer : GradientTextureBase {
#pragma warning disable 649
  // ReSharper disable FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
    [SerializeField, NotNull] SpriteRenderer spriteRenderer;
  // ReSharper restore FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
#pragma warning restore 649

    protected override void setTexture(Texture2D texture) => spriteRenderer.sprite = texture.toSprite();
  }
}
