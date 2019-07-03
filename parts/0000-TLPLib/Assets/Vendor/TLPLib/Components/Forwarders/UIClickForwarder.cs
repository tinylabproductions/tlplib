using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.unity_serialization;
using JetBrains.Annotations;
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
  [Serializable, PublicAPI] public class UnityOptionUIClickForwarder : UnityOption<UIClickForwarder> { }
}
