using System;
using com.tinylabproductions.TLPLib.Components;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using Coroutine = com.tinylabproductions.TLPLib.Concurrent.Coroutine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class GameObjectExts {
    public const float DOUBLE_CLICK_INTERVAL = 0.25f;

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

    public static GameObject setParent(this GameObject go, Transform parent, bool worldPositionStays = false) {
      go.transform.SetParent(parent, worldPositionStays);
      return go;
    }

    public static GameObject setParent(this GameObject go, GameObject parent, bool worldPositionStays = false) {
      return go.setParent(parent.transform, worldPositionStays);
    }

    public static A instantiate<A>(
      this A obj, Vector3 position, Quaternion rotation
    ) where A : Object {
      return (A) Object.Instantiate(obj, position, rotation);
    }

    public static A instantiate<A>(this A obj) where A : Object {
      return (A) Object.Instantiate(obj);
    }

    public static Coroutine everyFrame(this GameObject go, Fn<bool> f) {
      return ASync.EveryFrame(go, f);
    }

    public static Coroutine everyFrame(this GameObject go, Act a) {
      return go.everyFrame(() => { a(); return true; });
    }

    public static A EnsureComponent<A>(this GameObject go) where A : Component 
    { return go.GetComponent<A>() ?? go.AddComponent<A>(); }

    public static IObservable<Vector2> mouseDrag(this GameObject go) {
      return go.EnsureComponent<DragObservable>().dragDelta;
    }

    public static IObservable<Vector2> scrollWheel(this GameObject go) {
      return go.EnsureComponent<ScrollWheelObservable>().scrollDelta;
    }

    public static IObservable<float> pinch(this GameObject go) {
      return go.EnsureComponent<PinchObservable>().pinchDelta;
    }

    public static IObservable<KeyCode> keyDown(this GameObject go, KeyCode key) {
      return go.AddComponent<OnKeyDownForwarder>().init(key).onKeyDown;
    }

    public static IObservable<Unit> click(this GameObject go, bool uguiBlocks = true) {
      return go.AddComponent<OnMouseClickForwarder>().init(uguiBlocks).onMouseClick;
    }

    public static IObservable<Vector3> clickCoords(
      this GameObject go, bool uguiBlocks = true
    ) {
      return go.AddComponent<OnMouseClickCoordsForwarder>().init(uguiBlocks).onMouseClick;
    }

    public static IObservable<Unit> doubleClick(
      this GameObject go, float intervalS = DOUBLE_CLICK_INTERVAL, bool uguiBlocks = true
    ) { return go.click(uguiBlocks).withinTimeframe(2, intervalS).discardValue(); }
  }
}
