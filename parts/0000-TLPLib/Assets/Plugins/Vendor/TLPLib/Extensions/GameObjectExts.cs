using System;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using UnityEngine;
using Coroutine = com.tinylabproductions.TLPLib.Concurrent.Coroutine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class GameObjectExts {
    public static void changeAlpha(this GameObject go, float alpha) {
      foreach (var childRenderer in go.GetComponentsInChildren<Renderer>()) {
        var material = childRenderer.material;
        var c = material.color;
        material.color = new Color(c.r, c.g, c.b, alpha);
      }
    }

    public static void doRecursively(this GameObject go, Act<GameObject> act) {
      act(go);
      go.transform.doRecursively(t => act(t.gameObject));
    }

    public static void setLayerRecursively(this GameObject go, int layer) {
      void setLayer(Transform t) {
        t.gameObject.layer = layer;
        var childCount = t.childCount;
        for (var idx = 0; idx < childCount; idx++) {
          setLayer(t.GetChild(idx));
        }
      }
      
      setLayer(go.transform);
    }

    [PublicAPI]
    public static void replaceWith(this GameObject go, GameObject replacement) {
      var gt = go.transform;
      var rt = replacement.transform;
      var siblingIndex = gt.GetSiblingIndex();
      rt.parent = gt.parent;
      rt.position = gt.position;
      rt.rotation = gt.rotation;
      rt.localScale = gt.localScale;
      Object.Destroy(go);
      rt.SetSiblingIndex(siblingIndex);
    }

    public static Coroutine everyFrame(this GameObject go, Fn<bool> f) => ASync.EveryFrame(go, f);

    public static Coroutine everyFrame(this GameObject go, Action a) =>
      go.everyFrame(() => { a(); return true; });

    public static IObservable<Unit> onMouseDown(this GameObject go) =>
      go.onEvent<Unit, OnMouseDownForwarder>();

    public static IObservable<Unit> onMouseUp(this GameObject go) =>
      go.onEvent<Unit, OnMouseUpForwarder>();

    public static IObservable<A> onEvent<A, Forwarder>(this GameObject go) where Forwarder : EventForwarder<A> =>
      go.EnsureComponent<Forwarder>().onEvent;

    public static A EnsureComponent<A>(this GameObject go) where A : Component {
      // We can't use ?? operator here, because this operator is not overloaded in Unity Object and
      // it does not check if the object exists on the native side like the == operator does.
      var comp = go.GetComponent<A>();
      return comp ? comp : go.AddComponent<A>();
    }

    public static Option<A> GetComponentSafe<A>(this GameObject go) where A : Component =>
      go.GetComponent<A>().opt();

    public static Either<ErrorMsg, A> GetComponentSafeE<A>(this GameObject go) where A : Component {
      var res = go.GetComponentSafe<A>();
      return
        res.isNone
        ? (Either<ErrorMsg, A>) new ErrorMsg($"Can't find component {typeof(A)} on '{go}'")
        : res.__unsafeGetValue;
    }

    public static string nameOrNull(this GameObject go) =>
      go ? go.name : "none";

    // Modified from unity decompiled dll.
    // Added includeInactive parameter.
    public static T getComponentInChildren<T>(this GameObject go, bool includeInactive) where T : Component {
      if (includeInactive || go.activeInHierarchy) {
        var component = go.GetComponent<T>();
        if (component != null)
          return component;
      }
      var transform = go.transform;
      if (transform != null) {
        foreach (Component component in transform) {
          var componentInChildren = component.gameObject.getComponentInChildren<T>(includeInactive);
          if (componentInChildren != null)
            return componentInChildren;
        }
      }
      return null;
    }
  }
}
