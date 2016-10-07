using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIDownUpForwarder : UIBehaviour, IPointerDownHandler, IPointerUpHandler {
    readonly Subject<Unit> _onDown = new Subject<Unit>();
    readonly Subject<Unit> _onUp = new Subject<Unit>();
    public IObservable<Unit> onDown => _onDown;
    public IObservable<Unit> onUp => _onUp;
    public bool isDown { get; private set; }

    public void OnPointerDown(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive()) {
        _onDown.push(new Unit());
        isDown = true;
      }
    }

    public void OnPointerUp(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive()) {
        _onUp.push(new Unit());
        isDown = false;
      }
    }
  }
}
