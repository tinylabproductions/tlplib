using System;
using com.tinylabproductions.TLPLib.Components;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.dispose;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class UIExts {
    public static IRxObservable<Unit> uiClick(this UIBehaviour elem) => elem.gameObject.uiClick();
    public static IRxObservable<Unit> uiClick(this GameObject go) => go.EnsureComponent<UIClickForwarder>().onClick;
    public static IRxObservable<PointerEventData> uiDown(this GameObject go) => go.EnsureComponent<UIDownUpForwarder>().onDown;
    public static IRxObservable<PointerEventData> uiUp(this GameObject go) => go.EnsureComponent<UIDownUpForwarder>().onUp;

    /// <summary>
    /// Returns the <see cref="TimeSpan"/> (which is calculated from <see cref="ITimeContext"/>) between the down
    /// event and up event.
    /// </summary>
    public static IRxObservable<TimeSpan> uiDownUp(this GameObject go, ITimeContext timeContext) {
      var mapper = new Func<PointerEventData, TimeSpan>(_ => timeContext.passedSinceStartup);

      var downAt = go.uiDown().map(mapper);
      var upAt = go.uiUp().map(mapper);
      return downAt
        .zip(upAt, (downAt, upAt) =>
          // We need to filter this to prevent an event firing in the case of UP -> DOWN event sequence. 
          upAt >= downAt ? Some.a(upAt - downAt) : None._
        )
        .collect(_ => _);
    }
  }
}
