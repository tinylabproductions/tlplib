using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.reactive;

using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components {
  public class UIClickForwarder : UIBehaviour, IPointerClickHandler {
    Subject<Unit> _onClick = new Subject<Unit>();
    public IRxObservable<Unit> onClick => _onClick;

    public void OnPointerClick(PointerEventData eventData) {
      if (eventData.button == PointerEventData.InputButton.Left && IsActive())
        _onClick.push(F.unit);
    }

    public void reset() {
      _onClick = new Subject<Unit>();
    }
  }

  [Serializable, PublicAPI] public class UIClickForwarderPrefab : TagPrefab<UIClickForwarder> {}
}
