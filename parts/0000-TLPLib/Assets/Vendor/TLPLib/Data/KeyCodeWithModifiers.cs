using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Record, PublicAPI] public readonly partial struct KeyCodeWithModifiers {
    public readonly KeyCode keyCode;
    public readonly bool shift, alt, ctrl;

    bool modifiersValid =>
      (!shift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
      && (!alt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr))
      && (!ctrl || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
    
    public bool getKey => Input.GetKey(keyCode) && modifiersValid;
    public bool getKeyDown => Input.GetKeyDown(keyCode) && modifiersValid;
    public bool getKeyUp => Input.GetKeyUp(keyCode) && modifiersValid;
  }
}