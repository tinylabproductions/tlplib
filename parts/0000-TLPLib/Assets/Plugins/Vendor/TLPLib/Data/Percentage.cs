using System;
using System.Collections.Generic;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  [Record, Serializable]
  public partial struct Percentage : OnObjectValidate {
    // [0, 1]
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    [SerializeField, PublicAccessor, Help(HelpType.Info, "[0.0, 1.0]")] float _value;

    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      if (_value < 0f || _value > 1f)
        yield return new ErrorMsg($"Expected percentage value {_value} to be in range [0.0, 1.0]");
    }
  }
}