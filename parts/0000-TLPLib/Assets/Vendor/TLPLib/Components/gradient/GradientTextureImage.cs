using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public class GradientTextureImage : GradientTextureBase {
#pragma warning disable 649
  // ReSharper disable FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
    [SerializeField, NotNull] Image image;
  // ReSharper restore FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
#pragma warning restore 649 

    protected override void setTexture(Texture2D texture) => image.sprite = texture.toSprite();
  }
}
