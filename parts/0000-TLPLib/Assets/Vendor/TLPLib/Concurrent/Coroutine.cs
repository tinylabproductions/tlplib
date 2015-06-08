using System.Collections;
using UnityEngine;
using UECoroutine = UnityEngine.Coroutine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public struct Coroutine {
    public readonly UECoroutine backing;
    public readonly MonoBehaviour behaviour;
    public readonly IEnumerator enumerator;

    public Coroutine(MonoBehaviour behaviour, IEnumerator enumerator) {
      this.behaviour = behaviour;
      backing = behaviour.StartCoroutine(enumerator);
      this.enumerator = enumerator;
    }

    public void stop() {
      if (behaviour) behaviour.StopCoroutine(enumerator);
    }
  }
}
