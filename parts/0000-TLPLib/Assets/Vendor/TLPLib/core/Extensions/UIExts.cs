using com.tinylabproductions.TLPLib.Components;
using JetBrains.Annotations;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class UIExts {
    public static IRxObservable<Unit> uiClick(this UIBehaviour elem) => elem.gameObject.uiClick();
    public static IRxObservable<Unit> uiClick(this GameObject go) => go.EnsureComponent<UIClickForwarder>().onClick;
    public static IRxObservable<Unit> uiDown(this GameObject go) => go.EnsureComponent<UIDownUpForwarder>().onDown;
    public static IRxObservable<Unit> uiUp(this GameObject go) => go.EnsureComponent<UIDownUpForwarder>().onUp;
  }
}
