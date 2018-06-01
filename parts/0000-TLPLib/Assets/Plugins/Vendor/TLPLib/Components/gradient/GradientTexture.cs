using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public class GradientTexture : MonoBehaviour, IMB_Start {
    [SerializeField, NotNull] int textureSize = 128;
    [SerializeField, NotNull] Image textureTarget;
    [SerializeField, NotNull] Gradient gradient = new Gradient();
    [SerializeField, NotNull] Direction direction = Direction.Horizontal;

    enum Direction : byte { Vertical, Horizontal }

    public void Start() => generate();
    
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
      textureTarget.sprite = texture.toSprite();
    }
  }
}
