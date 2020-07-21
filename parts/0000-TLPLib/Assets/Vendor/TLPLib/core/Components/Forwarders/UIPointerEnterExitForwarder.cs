using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIPointerEnterExitForwarder : UIBehaviour, IPointerEnterHandler, IPointerExitHandler {

    readonly Subject<Unit> _onEnter = new Subject<Unit>();
    readonly Subject<Unit> _onExit = new Subject<Unit>();
    public IRxObservable<Unit> onEnter => _onEnter;
    public IRxObservable<Unit> onExit => _onExit;
    public bool isEntered { get; private set; }

    public void OnPointerEnter(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive()) {
        _onEnter.push(new Unit());
        isEntered = true;
      }
    }

    public void OnPointerExit(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive()) {
        _onExit.push(new Unit());
        isEntered = false;
      }
    }
  }
}
