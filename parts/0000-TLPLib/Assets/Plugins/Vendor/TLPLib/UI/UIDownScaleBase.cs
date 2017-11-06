using System;
using Assets.Code.Utils;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
namespace com.tinylabproductions.TLPLib.UI {
  public abstract class UIDownScaleBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IMB_Awake {
    public float scale = 1.1f;
    [Header("Optional, default to this.transform")]
    public Transform target;

    protected Vector3 previous;
    protected bool isDown;
    protected new SinusoidScale animation;

    public abstract Vector3 targetLocalScale { get; set; }
    public abstract Action onPointerUp { get; }

    public void Awake() {
      if (!target) target = transform;
      animation = target.GetComponent<SinusoidScale>();
    }

    public void OnPointerDown(PointerEventData eventData) {
      if (eventData.button != PointerEventData.InputButton.Left) return;
      pointerDown();
    }

    public void pointerDown() {
      if (!isDown) {
        isDown = true;
        previous = targetLocalScale;
        targetLocalScale *= scale;
        if (animation != null) animation.enabled = !isDown;
      }
    }

    public void OnPointerUp(PointerEventData eventData) {
      if (eventData.button != PointerEventData.InputButton.Left) return;
      pointerUp();
    }

    public virtual void pointerUp() {
      if (isDown) {
        isDown = false;
        targetLocalScale = previous;
        if (animation != null) animation.enabled = !isDown;
        onPointerUp();
      }
    }
  }
}
