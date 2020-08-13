using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIDownUpForwarder : PointerDownUp, IMB_OnDisable {
    readonly Subject<PointerEventData>
      _onDown = new Subject<PointerEventData>(),
      _onUp = new Subject<PointerEventData>();
    public IRxObservable<PointerEventData> onDown => _onDown;
    public IRxObservable<PointerEventData> onUp => _onUp;
    public bool isDown { get; private set; }

    void up(PointerEventData eventData) {
      _onUp.push(eventData);
      isDown = false;
    }

    protected override void onPointerDown(PointerEventData eventData) {
      if (isActiveAndEnabled) {
        _onDown.push(eventData);
        isDown = true;
      }
    }

    protected override void onPointerUp(PointerEventData eventData) {
      if (isActiveAndEnabled)
        up(eventData);
    }

    public void OnDisable() {
      if (isDown) {
        foreach (var data in pointerData) {
          up(data);
        }
      }
    }
  }
}
