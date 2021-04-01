using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Components.ui;
using pzd.lib.reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIDownUpForwarder : PointerDownUp, IMB_OnDisable {
    public readonly struct OnUpData {
      public enum UpType: byte { onPointerUp, OnDisable }

      public readonly PointerEventData eventData;
      public readonly UpType type;

      public OnUpData(PointerEventData eventData, UpType type) {
        this.eventData = eventData;
        this.type = type;
      }
    }

    readonly Subject<PointerEventData> _onDown = new();
    readonly Subject<OnUpData> _onUp = new();
    public IRxObservable<PointerEventData> onDown => _onDown;
    public IRxObservable<OnUpData> onUp => _onUp;
    public bool isDown { get; private set; }

    void up(OnUpData eventData) {
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
        up(new OnUpData(eventData, OnUpData.UpType.onPointerUp));
    }

    public void OnDisable() {
      if (isDown) {
        foreach (var data in pointerData) {
          up(new OnUpData(data, OnUpData.UpType.OnDisable));
        }
      }
    }
  }
}
