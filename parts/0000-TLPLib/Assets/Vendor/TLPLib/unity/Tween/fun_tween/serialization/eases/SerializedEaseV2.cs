using System;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using pzd.lib.utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  [Serializable, InlineProperty]
  public partial struct SerializedEaseV2 : ISkipObjectValidationFields, Invalidatable {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [
      SerializeField, HideLabel, HorizontalGroup, OnValueChanged(nameof(complexChanged)), PublicAccessor, 
      HideInInspector
    ] 
    bool _isComplex;
    [
      SerializeField, HideLabel, HorizontalGroup, OnValueChanged(nameof(invalidate)), 
      ShowIf(nameof(isSimple), animate: false), HideInInspector
    ] 
    SimpleSerializedEase _simple;
    [
      SerializeReference, NotNull, HideLabel, HorizontalGroup, OnValueChanged(nameof(invalidate)),
      ShowIf(nameof(isComplex), animate: false), InlineProperty
    ]
    IComplexSerializedEase _complex;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    void complexChanged() {
      // ReSharper disable AssignNullToNotNullAttribute
      if (isSimple) _complex = default;
      // ReSharper restore AssignNullToNotNullAttribute
    }
    
    [PublicAPI] public bool isSimple => !_isComplex;

    Ease _ease;
    [PublicAPI] public Ease ease => _ease ??= _isComplex ? _complex.ease : _simple.toEase();

    public override string ToString() => 
      _isComplex 
        ? (_complex?.easeName ?? "not set") 
        : _simple.ToString();

    public string[] blacklistedFields() => 
      _isComplex
        ? new [] { nameof(_simple) }
        : new [] { nameof(_complex) };

    public interface IComplexSerializedEase {
      public string easeName { get; }
      public void invalidate();
      public Ease ease { get; }
    }
    
    public void invalidate() {
      _ease = null;
      _complex?.invalidate();
      editor_invalidate();
    }

    partial void editor_invalidate();
  }
  
  [Serializable] public class ComplexEase_AnimationCurve : SerializedEaseV2.IComplexSerializedEase {
    [SerializeField, NotNull] AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
    
    public string easeName => nameof(AnimationCurve);
    public void invalidate() { }
    public Ease ease => _curve.Evaluate;
  }
  
  [Serializable] public class ComplexEase_Punch : SerializedEaseV2.IComplexSerializedEase {
    [
      SerializeField, 
      Tooltip("Indicates how much will the punch vibrate")
    ] int _vibrato = 10;

    [
      SerializeField, Range(0, 1),
      Tooltip(
        @"Represents how much the vector will go beyond the starting position when bouncing backwards.
1 creates a full oscillation between the direction and the opposite decaying direction,
while 0 oscillates only between the starting position and the decaying direction"
      )
    ] float _elasticity = 1;
    
    public string easeName => $"Punch(v: {_vibrato}, e: {_elasticity})";
    public void invalidate() { }
    public Ease ease => Eases.punch(vibrato: _vibrato, elasticity: _elasticity);
  }
  
  [Serializable] public class ComplexEase_Shake : SerializedEaseV2.IComplexSerializedEase {
    [SerializeField, NotNull] AnimationCurve 
      _intensityOverTime = AnimationCurve.Linear(0, 0, 1, 1),
      _amplitudeOverTime = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] float _intensityMulti = 20;
    
    public string easeName => $"Shake(s: {_intensityMulti})";
    public void invalidate() {}
    public Ease ease => p => Mathf.Sin(p * _intensityOverTime.Evaluate(p) * _intensityMulti) * _amplitudeOverTime.Evaluate(p);
  }

  [Record] partial class SelectedEase {
    public readonly Either<SimpleSerializedEase, Type> value;
  }
}