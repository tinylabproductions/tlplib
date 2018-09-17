using UnityEngine;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  public partial class HideByAttribute : PropertyAttribute {
    [PublicAccessor] readonly string _boolSourceField;
    [PublicAccessor] readonly bool _hideInInspector;
    
    public HideByAttribute(string boolSourceField) {
      _boolSourceField = boolSourceField;
      _hideInInspector = true;
    }

    public HideByAttribute(string boolSourceField, bool hideInInspector) {
      _boolSourceField = boolSourceField;
      _hideInInspector = hideInInspector;
    }
  }
}