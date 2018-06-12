using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public abstract class GradientTextureBase : MonoBehaviour, IMB_Start {
    [SerializeField] int textureSize = 128;
    [SerializeField, NotNull] Gradient gradient = new Gradient();
    [SerializeField] Direction direction = Direction.Horizontal;

    enum Direction : byte { Vertical = 0, Horizontal = 1 }

    public void Start() => generate();

    protected abstract void setTexture(Texture2D texture);
    
    [Inspect]
    void generate() {
      var texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
      var pixels = new Color[textureSize * textureSize];

      if (direction == Direction.Horizontal)
        for (int x = 0; x < textureSize; x++) {
          var c = gradient.Evaluate(x / (float) textureSize);
          for (int y = 0; y < textureSize; y++) {
            pixels[x + y * textureSize] = c;
          }
        }
      else if (direction == Direction.Vertical)
        for (int y = 0; y < textureSize; y++) {
          var c = gradient.Evaluate(y / (float) textureSize);
          for (int x = 0; x < textureSize; x++) {
            pixels[x + y * textureSize] = c;
          }
        }

      texture.SetPixels(pixels);
      texture.Apply();
      setTexture(texture);
    }
  }
}
