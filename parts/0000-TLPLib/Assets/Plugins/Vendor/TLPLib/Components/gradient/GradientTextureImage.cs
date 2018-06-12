using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public class GradientTextureImage : GradientTextureBase {
    // ReSharper disable once NotNullMemberIsNotInitialized
    [SerializeField, NotNull] Image image;

    protected override void setTexture(Texture2D texture) => image.sprite = texture.toSprite();
  }
}
