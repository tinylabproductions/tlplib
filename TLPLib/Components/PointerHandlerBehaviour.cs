using System;
using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Components.Forwarders;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  class PointerHandlerBehaviour : MonoBehaviour {
    const int MOUSE_BTN_FIRST = 0;
    static PointerHandlerBehaviour _instance;

    public static PointerHandlerBehaviour instance { get {
      init();
      return _instance;
    } }

    public static void init() {
      if (_instance == null) {
        var go = new GameObject("Pointer Handler");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<PointerHandlerBehaviour>();
      }
    }

    [UsedImplicitly] void Update() {
      if (Input.touchCount != 0) {
        var touch = Input.GetTouch(0);
        var hitDataOpt = raycast(touch.position);
        if (hitDataOpt.isDefined) {
          var hitData = hitDataOpt.get;
          if (touch.phase == TouchPhase.Began) onPointerDown(hitData._1, hitData._2);
          if (touch.phase == TouchPhase.Ended) onPointerUp(hitData._1, hitData._2);
        }
      }
      else {
        var btnDown = Input.GetMouseButtonDown(MOUSE_BTN_FIRST);
        var btnUp = Input.GetMouseButtonUp(MOUSE_BTN_FIRST);
        if (btnDown || btnUp) {
          var hitDataOpt = raycast(Input.mousePosition);
          if (hitDataOpt.isDefined) {
            var hitData = hitDataOpt.get;
            if (btnDown) onPointerDown(hitData._1, hitData._2);
            if (btnUp) onPointerUp(hitData._1, hitData._2);
          }
        }
      }
    }

    static Option<Tpl<GameObject, Vector3>> raycast(Vector2 position) {
      RaycastHit hit;
      var ray = Camera.main.ScreenPointToRay(position);
      return Physics.Raycast(ray, out hit)
        ? F.t(hit.transform.gameObject, hit.point).some() 
        : F.none<Tpl<GameObject, Vector3>>();
    }

    static void onPointerDown(GameObject hitObject, Vector3 hitPoint) {
      Log.trace(string.Format("onPointerDown on {0} @ {1}", hitObject, hitPoint));
      var cmp = hitObject.GetComponent<OnPointerClickBase>();
      if (cmp != null) cmp.onPointerDown(hitPoint);
    }

    static void onPointerUp(GameObject hitObject, Vector3 hitPoint) {
      Log.trace(string.Format("onPointerUp on {0} @ {1}", hitObject, hitPoint));
      var cmp = hitObject.GetComponent<OnPointerClickBase>();
      if (cmp != null) cmp.onPointerUp(hitPoint);
    }
  }
}
