using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public class GradientTextureCustomImage : GradientTextureBase {
    // ReSharper disable once NotNullMemberIsNotInitialized
    [SerializeField, NotNull] CustomImage image;

    protected override void setTexture(Texture2D texture) => image.sprite = texture.toSprite();
  }
}
