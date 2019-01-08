using System;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public abstract class InfoWindow : MonoBehaviour, IMB_Awake, IMB_OnGUI {

    protected abstract Rect initialRect();
    protected abstract Fn<string> text { get; }
    protected abstract Fn<Color> updateColorFn { get; }
    protected abstract bool dragAllowed { get; }

    Rect startRect;
    GUIStyle style;

    public void Awake() { startRect = initialRect(); }

    public void OnGUI() {
      if (style == null) {
        style = new GUIStyle(GUI.skin.label) {
          normal = {textColor = Color.white},
          alignment = TextAnchor.MiddleCenter
        };
      }

     GUI.color = updateColorFn();

      startRect = GUI.Window(
        id: 0,
        clientRect: startRect,
        func: _ => {
          GUI.Label(new Rect(0, 0, startRect.width, startRect.height), text(), style);
          if (dragAllowed) GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        },
        text: ""
      );
    }
  }
}