using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace Plugins.Vendor.TLPLib.Binding {
  public class ToggleBinding : MonoBehaviour {
    [Header("Optional")]
    public GameObject whenEnabled, whenDisabled;

    public virtual void setEnabled(bool enabled) {
      if (whenEnabled) {
        whenEnabled.SetActive(enabled);
      }
      if (whenDisabled) {
        whenDisabled.SetActive(!enabled);
      }
    }

    public IObservable<Unit> uiClick => gameObject.uiClick();
  }
}
