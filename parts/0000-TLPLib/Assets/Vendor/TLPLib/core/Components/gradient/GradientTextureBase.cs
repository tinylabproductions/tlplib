
using com.tinylabproductions.TLPLib.Components.Interfaces;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  [ExecuteInEditMode]
  public abstract class GradientTextureBase : MonoBehaviour, IMB_Awake {
    [SerializeField] int textureSize = 128;
    [SerializeField, NotNull] Gradient gradient = new Gradient();
    [SerializeField] Direction direction = Direction.Horizontal;

    enum Direction : { Vertical = 0, Horizontal = 1 }

    public void Awake() => generate();

    protected abstract void setTexture(Texture2D texture);
    
    [Button]
    void generate() {
      var textureSizeX = direction == Direction.Horizontal ? textureSize : 1;
      var textureSizeY = direction == Direction.Vertical ? textureSize : 1;
      var texture = new Texture2D(textureSizeX, textureSizeY, TextureFormat.ARGB32, false);
      var pixels = new Color[textureSize];

      for (var x = 0; x < textureSize; x++) {
        pixels[x] = gradient.Evaluate(x / (float) textureSize);
      }

      texture.wrapMode = TextureWrapMode.Clamp;
      texture.SetPixels(pixels);
      texture.Apply();
      setTexture(texture);
    }
  }
}
