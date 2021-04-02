using System;
using com.tinylabproductions.TLPLib.Components;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class UIExts {
    public static IRxObservable<Unit> uiClick(this UIBehaviour elem) => elem.gameObject.uiClick();
    public static IRxObservable<Unit> uiClick(this GameObject go) => go.EnsureComponent<UIClickForwarder>().onClick;
    public static IRxObservable<PointerEventData> uiDown(this GameObject go) => go.EnsureComponent<UIDownUpForwarder>().onDown;
    public static IRxObservable<UIDownUpForwarder.OnUpData> uiUp(this GameObject go) => go.EnsureComponent<UIDownUpForwarder>().onUp;

    /// <summary>
    /// Returns the <see cref="TimeSpan"/> (which is calculated from <see cref="ITimeContext"/>) between the down
    /// event and up event.
    /// </summary>
    public static IRxObservable<UIDownUpResult> uiDownUp(this GameObject go, ITimeContext timeContext) {
      TimeSpan downMapper(PointerEventData pointerEventData) => timeContext.passedSinceStartup;
      (TimeSpan at, UIDownUpForwarder.OnUpData data) upMapper(UIDownUpForwarder.OnUpData pointerEventData)
        => (timeContext.passedSinceStartup, pointerEventData);

      var downAt = go.uiDown().map(downMapper);
      var upAt = go.uiUp().map(upMapper);
      return downAt
        .zip(upAt, (downAt, up) =>
          // We need to filter this to prevent an event firing in the case of UP -> DOWN event sequence. 
          up.at >= downAt ? Some.a(new UIDownUpResult(up.at - downAt, up.data)) : None._
        )
        .collect(_ => _);
    }
  }

  public readonly struct UIDownUpResult {
    public readonly TimeSpan pressedDuration;
    public readonly UIDownUpForwarder.OnUpData upData;

    public UIDownUpResult(TimeSpan pressedDuration, UIDownUpForwarder.OnUpData upData) {
      this.pressedDuration = pressedDuration;
      this.upData = upData;
    }
  }
}
