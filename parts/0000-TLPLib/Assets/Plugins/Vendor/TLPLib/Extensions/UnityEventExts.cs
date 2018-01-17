using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UnityEventExts {
    public static ISubscription subscribe(this UnityEvent<Vector2> rectEvent, UnityAction<Vector2> act) {
      rectEvent.AddListener(act);
      return new Subscription(() => rectEvent.RemoveListener(act));
    }
  }
}