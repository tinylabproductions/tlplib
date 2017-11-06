﻿using System;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
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
      go.doRecursively(o => o.layer = layer);
    }

    public static void replaceWith(this GameObject go, GameObject replacement) {
      replacement.transform.parent = go.transform.parent;
      replacement.transform.position = go.transform.position;
      replacement.transform.rotation = go.transform.rotation;
      replacement.transform.localScale = go.transform.localScale;
      Object.Destroy(go);
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

    public static A EnsureComponent<A>(this GameObject go) where A : Component => 
      go.GetComponent<A>() ?? go.AddComponent<A>();
    
    public static Option<A> GetComponentSafe<A>(this GameObject go) where A : Component =>
      go.GetComponent<A>().opt();

    public static Either<ErrorMsg, A> GetComponentSafeE<A>(this GameObject go) where A : Component {
      var res = go.GetComponentSafe<A>();
      return 
        res.isNone 
        ? (Either<ErrorMsg, A>) new ErrorMsg($"Can't find component {typeof(A)} on '{go}'") 
        : res.__unsafeGetValue;
    }

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
