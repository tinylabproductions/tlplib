using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIClickForwarder : UIBehaviour, IPointerClickHandler {
    Subject<Unit> _onClick = new Subject<Unit>();
    public IObservable<Unit> onClick => _onClick;

    public void OnPointerClick(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive())
        _onClick.push(F.unit);
    }

    public void reset() {
      _onClick = new Subject<Unit>();
    }
  }
}
