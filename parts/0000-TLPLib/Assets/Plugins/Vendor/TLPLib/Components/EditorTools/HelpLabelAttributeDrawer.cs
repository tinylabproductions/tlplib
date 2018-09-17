using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  [CustomPropertyDrawer(typeof(HelpLabelAttribute))]
  public class HelpLabelAttributeDrawer : DecoratorDrawer {

    public override float GetHeight() {
      if (!(attribute is HelpLabelAttribute helpBoxAttribute)) return base.GetHeight();
      var helpBoxStyle = GUI.skin != null ? GUI.skin.GetStyle("helpbox") : null;
      return helpBoxStyle == null
        ? base.GetHeight()
        : Mathf.Max(
          40f,
          helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.text), EditorGUIUtility.currentViewWidth) + 4
        );
    }

    public override void OnGUI(Rect position) {
      if (!(attribute is HelpLabelAttribute helpBoxAttribute)) return;
      EditorGUI.HelpBox(position, helpBoxAttribute.text, getMessageType(helpBoxAttribute.messageType));
    }

    static MessageType getMessageType(HelpBoxMessageType helpBoxMessageType) {
      switch (helpBoxMessageType) {
        case HelpBoxMessageType.Info: return MessageType.Info;
        case HelpBoxMessageType.Warning: return MessageType.Warning;
        case HelpBoxMessageType.Error: return MessageType.Error;
        default: return MessageType.None;
      }
    }
  }
}