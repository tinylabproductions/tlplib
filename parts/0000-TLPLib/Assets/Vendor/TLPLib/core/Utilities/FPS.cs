using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class FPS {
    public const float frequency = 0.5f;

    public static IRxVal<float> fps { get { return _fps; } }

    private static readonly IRxRef<float> _fps = RxRef.a(-1f);

    private static float accum;
    private static int frames;

    static FPS() {
      ASync.StartCoroutine(update());
      ASync.StartCoroutine(calculate());
    }

    static IEnumerator update() {
      while (true) {
        accum += Time.timeScale / Time.deltaTime;
        ++frames;
        yield return null;
      }
    }

    static IEnumerator calculate() {
      var wait = new WaitForSeconds(frequency);
      while (true) {
        _fps.value = accum / frames;
        accum = 0.0F;
        frames = 0;

        yield return wait;
      }
    }
  }
}
