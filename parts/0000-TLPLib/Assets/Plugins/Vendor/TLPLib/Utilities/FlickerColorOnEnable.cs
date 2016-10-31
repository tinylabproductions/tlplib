﻿using System.Collections;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.Plugins.Vendor.TLPLib.Utilities {
  public class FlickerColorOnEnable : MonoBehaviour, IMB_Awake, IMB_OnEnable {
    public Color flickeringColor;
    public int ammountOfFlickers = 5;
    public float flickeringRate = .15f;

    public void Awake() { enabled = false; }

    public void OnEnable() { StartCoroutine(flicker()); }

    IEnumerator flicker() {
      var sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
      if (sprites.nonEmpty()) {
        var originalColors = sprites.map(_ => _.color);
        var wait = new WaitForSeconds(flickeringRate);

        for (var i = 0; i < ammountOfFlickers; i++) {
          foreach (var sprite in sprites) sprite.color = flickeringColor;
          yield return wait;

          for (var spriteIdx = 0; spriteIdx < sprites.Length; spriteIdx++)
            sprites[spriteIdx].color = originalColors[spriteIdx];
          yield return wait;
        }
      }
      enabled = false;
    }
  }
}