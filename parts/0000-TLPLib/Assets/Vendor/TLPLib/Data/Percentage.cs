using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  [Record, Serializable]
  public partial struct Percentage : OnObjectValidate {
    // [0, 1]
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    [SerializeField, PublicAccessor, InfoBox("[0.0, 1.0]")] float _value;

    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      if (_value < 0f || _value > 1f)
        yield return new ErrorMsg($"Expected percentage value {_value} to be in range [0.0, 1.0]");
    }

    public static Percentage operator *(Percentage p, float multiplier) =>
      new Percentage(Mathf.Clamp01(p._value * multiplier));
  }
}