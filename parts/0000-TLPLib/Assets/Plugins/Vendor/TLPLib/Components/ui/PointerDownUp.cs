using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.ui {
  public abstract class PointerDownUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    /// <summary>
    /// We need a list here, because:
    /// * If we made several touches and then released some of them, we need to
    ///   set isHeldDown to the last touch position we still have. Therefore we
    ///   cannot use a counter.
    /// * we can release touches in different order than we pressed. Therefore
    ///   we cannot use a stack.
    ///
    /// The amount of elements should be small (amount of simultaneous touches
    /// recognized by a device, which is usually less than 10), therefore the
    /// performance gains from switching to other data structure would be
    /// negligable here.
    /// </summary>
    protected readonly List<PointerEventData> pointerData = new List<PointerEventData>();

    public void OnPointerDown(PointerEventData eventData) {
      // There exists a bug
      // when you release and then press at the same frame, OnPointerUp doesn't fire,
      // so we skip OnPointerDown event, to not corrupt pointerData
      if (pointerData.Contains(eventData)) return;

      pointerData.Add(eventData);
      onPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData) {
      pointerData.Remove(eventData);
      onPointerUp(eventData);
    }

    protected abstract void onPointerDown(PointerEventData eventData);
    protected abstract void onPointerUp(PointerEventData eventData);
  }
}