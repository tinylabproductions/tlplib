using com.tinylabproductions.TLPLib.Components.Interfaces;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vendor.TLPLib.Components.ui {
  // https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
  public class ResizeToSafeAreaAnchors : UIBehaviour, IMB_Update {

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] RectTransform _rt;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    Rect lastSafeArea = new Rect (0, 0, 0, 0);

    protected override void Awake() => refresh();

    public void Update () => refresh();

    void refresh() {
      var safeArea = Screen.safeArea;
      if (safeArea != lastSafeArea) {
        lastSafeArea = safeArea;
        applySafeArea(safeArea);
      }
    }

    void applySafeArea(Rect r) {
      var anchorMin = r.position;
      var anchorMax = r.position + r.size;
      anchorMin.x /= Screen.width;
      anchorMin.y /= Screen.height;
      anchorMax.x /= Screen.width;
      anchorMax.y /= Screen.height;
      _rt.anchorMin = anchorMin;
      _rt.anchorMax = anchorMax;
    }
  }
}