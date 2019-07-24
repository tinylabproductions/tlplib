using System;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Record, PublicAPI, Serializable] public partial class KeyCodeWithModifiers {
#pragma warning disable 649
    [PublicAccessor, SerializeField] KeyCode _keyCode;
    [PublicAccessor, SerializeField] bool _shift, _alt, _ctrl;
#pragma warning restore 649

    bool modifiersValid =>
      (!_shift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
      && (!_alt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr))
      && (!_ctrl || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
    
    public bool getKey => Input.GetKey(_keyCode) && modifiersValid;
    public bool getKeyDown => Input.GetKeyDown(_keyCode) && modifiersValid;
    public bool getKeyUp => Input.GetKeyUp(_keyCode) && modifiersValid;
  }
}