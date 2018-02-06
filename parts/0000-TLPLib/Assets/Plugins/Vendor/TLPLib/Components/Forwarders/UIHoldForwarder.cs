using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIHoldForwarder : MonoBehaviour, IMB_Update, IPointerDownHandler, IPointerUpHandler {
    readonly IRxRef<Option<Vector2>> _isHeldDown = RxRef.a(F.none<Vector2>());
    public IRxVal<Option<Vector2>> isHeldDown => _isHeldDown;

    readonly Subject<Vector2> _onHoldEveryFrame = new Subject<Vector2>();
    public IObservable<Vector2> onHoldEveryFrame => _onHoldEveryFrame;

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
    readonly List<PointerEventData> pointerData = new List<PointerEventData>();
    
    public void Update() {
      if (pointerData.isEmpty()) return;
      
      // HOT CODE: We explicitly use indexing instead of LINQ .Last() to make
      // sure this performs well and does not do unnecessary checks.
      var lastPointer = pointerData[pointerData.Count - 1];
      _onHoldEveryFrame.push(lastPointer.position);
    }

    public void OnPointerDown(PointerEventData eventData) {
      pointerData.Add(eventData);
      _isHeldDown.value = eventData.position.some();
    }

    public void OnPointerUp(PointerEventData eventData) {
      pointerData.Remove(eventData);
      if (pointerData.isEmpty()) {
        _isHeldDown.value = _isHeldDown.value.none;
      }
    }
  }
}