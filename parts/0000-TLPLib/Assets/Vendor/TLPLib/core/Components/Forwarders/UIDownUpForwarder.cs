﻿using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIDownUpForwarder : PointerDownUp, IMB_OnDisable {
    readonly Subject<Unit>
      _onDown = new Subject<Unit>(),
      _onUp = new Subject<Unit>();
    public IRxObservable<Unit> onDown => _onDown;
    public IRxObservable<Unit> onUp => _onUp;
    public bool isDown { get; private set; }

    void up() {
      _onUp.push(new Unit());
      isDown = false;
    }

    protected override void onPointerDown(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && isActiveAndEnabled) {
        _onDown.push(new Unit());
        isDown = true;
      }
    }

    protected override void onPointerUp(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && isActiveAndEnabled)
        up();
    }

    public void OnDisable() {
      if (isDown)
        up();
    }
  }
}
