using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIClickForwarder : UIBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {
    private readonly Subject<Unit> _onClick = new Subject<Unit>();
    private readonly Subject<Unit> _onDown = new Subject<Unit>();
    private readonly Subject<Unit> _onUp = new Subject<Unit>();
    public IObservable<Unit> onClick { get { return _onClick; } }
    public IObservable<Unit> onDown { get { return _onDown; } }
    public IObservable<Unit> onUp { get { return _onUp; } }

    public void OnPointerClick(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive())
        _onClick.push(new Unit());
    }

    public void OnPointerDown(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive())
        _onDown.push(new Unit());
    }

    public void OnPointerUp(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive())
        _onDown.push(new Unit());
    }
  }
}
