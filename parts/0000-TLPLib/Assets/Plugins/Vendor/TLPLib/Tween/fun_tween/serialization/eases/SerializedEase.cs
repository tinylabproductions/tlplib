using System;
using com.tinylabproductions.TLPLib.attributes;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  [Serializable]
  public partial class SerializedEase : ISkipObjectValidationFields, Invalidatable {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField, ShowIf(nameof(validate)), PublicAccessor] bool _isComplex;
    [SerializeField, ShowIf(nameof(isSimple))] SimpleSerializedEase _simple;
    [SerializeField, ShowIf(nameof(isComplex)), TLPCreateDerived, NotNull] ComplexSerializedEase _complex;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    [PublicAPI] public SerializedEase(SimpleSerializedEase simple) {
      _isComplex = false;
      _simple = simple;
    }

    [PublicAPI] public SerializedEase(ComplexSerializedEase complex) {
      _isComplex = true;
      _complex = complex;
    }
    
    bool validate() {
      // ReSharper disable AssignNullToNotNullAttribute
      if (isSimple) _complex = default;
      else _simple = default;
      // ReSharper restore AssignNullToNotNullAttribute
      return true;
    }
    
    [PublicAPI] public bool isSimple => !_isComplex;

    Ease _ease;
    [PublicAPI] public Ease ease => _ease ?? (_ease = _isComplex ? _complex.ease : _simple.toEase());
    
    public void invalidate() {
      _ease = null;
      if (_complex) _complex.invalidate();
    }

    public override string ToString() => 
      _isComplex 
        ? (_complex ? _complex.easeName : "not set") 
        : _simple.ToString();

    public string[] blacklistedFields() => 
      _isComplex
        ? new [] { nameof(_simple) }
        : new [] { nameof(_complex) };
  }
}