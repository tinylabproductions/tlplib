using System;
using AdvancedInspector;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  [Serializable]
  public partial class SerializedTweenSequence {
    [Serializable]
    public partial struct Element {
      [SerializeField, Tooltip("in seconds"), PublicAccessor] float _at;
      [SerializeField, NotNull, CreateDerived] SerializedTweenSequenceElement _element;

      public override string ToString() => $"{_at}s: {_element}";
    }

    [SerializeField] Element[] _elements;
  }
}