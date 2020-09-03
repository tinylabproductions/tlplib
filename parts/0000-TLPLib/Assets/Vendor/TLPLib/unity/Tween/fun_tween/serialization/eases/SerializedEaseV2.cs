using System;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  [Serializable, InlineProperty]
  public partial struct SerializedEaseV2 : ISkipObjectValidationFields, Invalidatable {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    // TODO: implement complex eases
    [HideLabel, HorizontalGroup, OnValueChanged(nameof(complexChanged)), PublicAccessor] 
    [SerializeField] bool _isComplex;
    [HideLabel, HorizontalGroup, OnValueChanged(nameof(invalidate)), ShowIf(nameof(isSimple), animate: false)] 
    [SerializeField] SimpleSerializedEase _simple;
    [HideLabel, HorizontalGroup, OnValueChanged(nameof(invalidate)), ShowIf(nameof(isComplex), animate: false), InlineProperty]
    [SerializeField, NotNull, SerializeReference] IComplexSerializedEase _complex;
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
    
    public void invalidate() {
      _ease = null;
      _complex?.invalidate();
#if UNITY_EDITOR
      _preview = null;
#endif
    }

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
    
#if UNITY_EDITOR
    Texture2D _preview;
    [HideLabel, HorizontalGroup(Width = 50), ShowIf("displayPreview", animate: false), ShowInInspector, PreviewField]
    Texture2D preview => _preview ? _preview : _preview = (
      isSimple 
        ? SerializedEasePreview.editorPreview(_simple) 
        : (_complex != null ? SerializedEasePreview.generateTexture(ease) : null)
    );
    
    [UsedImplicitly] bool displayPreview => SerializedTweenTimelineV2.editorDisplayEasePreview;
#endif
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
}