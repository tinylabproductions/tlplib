using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UnityEventExts {
    public static ISubscription subscribe<A>(this UnityEvent<A> rectEvent, UnityAction<A> act) {
      rectEvent.AddListener(act);
      return new Subscription(() => rectEvent.RemoveListener(act));
    }
  }
}