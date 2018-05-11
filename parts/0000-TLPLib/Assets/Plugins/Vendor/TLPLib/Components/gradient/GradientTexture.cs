using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.gradient {

  [RequireComponent(typeof(Image))]
  public class GradientTexture: MonoBehaviour {

    [SerializeField] Texture2D texture;
    [SerializeField] int textureSize = 128;

    public enum Direction {
      Vertical, Horizontal
    }

    [SerializeField] Gradient gradient = new Gradient();
    [SerializeField] Direction direction = Direction.Horizontal;

    [Inspect]
    void generate() {

      texture = new Texture2D(textureSize, textureSize);
      if (direction == Direction.Horizontal)
        for (int x = 0; x < textureSize; x++) {
          for (int y = 0; y < textureSize; y++) {
            texture.SetPixel(x, y, gradient.Evaluate(x / (float)textureSize));
          }
        }
      else if (direction == Direction.Vertical)
        for (int x = 0; x < textureSize; x++) {
          for (int y = 0; y < textureSize; y++) {
            texture.SetPixel(x, y, gradient.Evaluate(y / (float)textureSize));
          }
        }

      GetComponent<Image>().sprite = texture.toSprite();
    }

  }

}