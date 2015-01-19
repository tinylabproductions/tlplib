using com.tinylabproductions.TLPLib.Components;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UIExts {
    public static IObservable<Unit> uiClick(this UIBehaviour elem) 
    { return elem.gameObject.uiClick(); }

    public static IObservable<Unit> uiClick(this GameObject go) 
    { return go.EnsureComponent<UIClickForwarder>().onClick; }
  }
}
