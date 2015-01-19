using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIClickForwarder : UIBehaviour, IPointerClickHandler {
    private readonly Subject<Unit> _onClick = new Subject<Unit>();
    public IObservable<Unit> onClick { get { return _onClick; } }

    public void OnPointerClick(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive())
        _onClick.push(F.unit);
    }
  }
}
