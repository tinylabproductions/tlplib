using System;
using System.Text;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.typeclasses;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  /// You can succinctly define <see cref="KeyCodeWithModifiers"/> with this:
  /// 
  /// <code>
  /// using static com.tinylabproductions.TLPLib.Data.KeyModifier;
  /// using static UnityEngine.KeyCode;
  /// 
  /// var keyCodeWithModifiers = Ctrl+Alt+Z;
  /// </code>
  [Record(GenerateConstructor = ConstructorFlags.None)] public partial struct KeyModifier {
    public static readonly KeyModifier 
      Ctrl = new KeyModifier(Val.Ctrl), Alt = new KeyModifier(Val.Alt), Shift = new KeyModifier(Val.Shift);
    
    [Flags] enum Val : byte { Ctrl = 1 << 1, Alt = 1 << 2, Shift = 1 << 3 }
    
    readonly Val val;
    KeyModifier(Val val) => this.val = val;

    public static KeyModifier operator +(KeyModifier m1, KeyModifier m2) => new KeyModifier(m1.val | m2.val);
    
    public static KeyCodeWithModifiers operator +(KeyModifier m, KeyCode key) => new KeyCodeWithModifiers(
      key, _shift: (m.val & Val.Shift) != 0, _ctrl: (m.val & Val.Ctrl) != 0, _alt: (m.val & Val.Alt) != 0
    );
  }
  
  [Record, PublicAPI, Serializable] public partial struct KeyCodeWithModifiers : IStr {
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

    public string asString() {
      var sb = new StringBuilder();
      if (_ctrl) sb.Append('c');
      if (_alt) sb.Append('a');
      if (_shift) sb.Append('s');
      if (_ctrl || _alt || _shift) sb.Append('+');
      sb.Append(_keyCode.ToString());
      return sb.ToString();
    }
  }
}