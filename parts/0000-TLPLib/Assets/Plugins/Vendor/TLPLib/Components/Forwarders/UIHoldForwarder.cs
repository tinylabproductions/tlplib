using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIHoldForwarder : MonoBehaviour, IMB_Update, IPointerDownHandler, IPointerUpHandler {
    readonly IRxRef<bool> _isHeldDown = RxRef.a(false);
    public IRxVal<bool> isHeldDown => _isHeldDown;

    readonly Subject<Unit> _onHold = new Subject<Unit>();
    public IObservable<Unit> onHold => _onHold;

    public void Update() {
      if (isHeldDown.value) _onHold.push(F.unit);
    }

    public void OnPointerDown(PointerEventData eventData) => 
      _isHeldDown.value = true;

    public void OnPointerUp(PointerEventData eventData) => 
      _isHeldDown.value = false;
  }
}