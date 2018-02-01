using System.Collections.Generic;
using System.Linq;
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

    readonly List<PointerEventData> pointerData = new List<PointerEventData>();
    
    public void Update() {
      if (isHeldDown.value)
        _onHoldEveryFrame.push(pointerData.Last().position);
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