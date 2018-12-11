using com.tinylabproductions.TLPLib.Components;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UIExts {
    public static IRxObservable<Unit> uiClick(this UIBehaviour elem)
    { return elem.gameObject.uiClick(); }

    public static IRxObservable<Unit> uiClick(this GameObject go)
    { return go.EnsureComponent<UIClickForwarder>().onClick; }

    public static IRxObservable<Unit> uiDown(this GameObject go)
    { return go.EnsureComponent<UIDownUpForwarder>().onDown; }

    public static IRxObservable<Unit> uiUp(this GameObject go)
    { return go.EnsureComponent<UIDownUpForwarder>().onUp; }
  }
}
