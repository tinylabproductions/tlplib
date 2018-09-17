using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  public enum HelpBoxMessageType {
    None,
    Info,
    Warning,
    Error
  }
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class
    | AttributeTargets.Struct, AllowMultiple = true)]
  public class HelpLabelAttribute : PropertyAttribute {

    public string text;
    public HelpBoxMessageType messageType;

    public HelpLabelAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None) {
      this.text = text;
      this.messageType = messageType;
    }
  }
  
  
}