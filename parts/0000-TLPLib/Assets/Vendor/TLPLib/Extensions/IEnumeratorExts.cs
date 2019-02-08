using System.Collections;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IEnumeratorExts {
    public static IEnumerator withDelay(this IEnumerator enumeratorNext, float seconds) {
      yield return new WaitForSeconds(seconds);
      yield return enumeratorNext;
    }
  }
}