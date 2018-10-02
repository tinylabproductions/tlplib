using System;
using AdvancedInspector;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateToString = false), Serializable]
  public partial struct Percentage {
    // [0, 1]
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    [SerializeField, PublicAccessor, Help(HelpType.Info, "[0.0, 1.0]")] float _value;

    public override string ToString() => $"{_value * 100}%";
  }
}