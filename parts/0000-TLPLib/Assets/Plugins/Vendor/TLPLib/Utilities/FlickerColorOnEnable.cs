using System.Collections;
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
      var opt = gameObject.GetComponentInChildren<SpriteRenderer>().opt();
      foreach (var op in opt) {
        var originalColor = op.color;

        for (var i = 0; i < ammountOfFlickers; i++) {
          setAllSpritesColor(flickeringColor);
          yield return new WaitForSeconds(flickeringRate);

          setAllSpritesColor(originalColor);
          yield return new WaitForSeconds(flickeringRate);
        }
      }
      enabled = false;
    }

    void setAllSpritesColor(Color color) {
      var opt = gameObject.GetComponentsInChildren<SpriteRenderer>().opt();
      foreach (var sprites in opt) {
        foreach (var sprite in sprites) sprite.color = color;
      }
    }
  }
}