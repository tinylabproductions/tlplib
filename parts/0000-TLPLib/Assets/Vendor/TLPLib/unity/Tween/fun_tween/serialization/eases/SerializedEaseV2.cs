using System;
using com.tinylabproductions.TLPLib.attributes;
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
    [HideInInspector]
    [HideLabel, HorizontalGroup, OnValueChanged(nameof(complexChanged)), PublicAccessor] 
    [SerializeField] bool _isComplex;
    [HideLabel, HorizontalGroup, OnValueChanged(nameof(invalidate)), ShowIf(nameof(isSimple))] 
    [SerializeField] SimpleSerializedEase _simple;
    [HideLabel, HorizontalGroup, OnValueChanged(nameof(invalidate)), ShowIf(nameof(isComplex)), TLPCreateDerived]
    [SerializeField, NotNull] IComplexSerializedEase _complex;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion
    
    [HideLabel, HorizontalGroup(Width = 50), ShowIf(nameof(displayPreview)), ShowInInspector, PreviewField]
    Texture2D _simplePreview => SerializedEasePreview.editorPreview(_simple);
    
    void complexChanged() {
      // ReSharper disable AssignNullToNotNullAttribute
      if (isSimple) _complex = default;
      // ReSharper restore AssignNullToNotNullAttribute
    }
    
    bool displayPreview => SerializedTweenTimelineV2.editorDisplayEasePreview && isSimple;
    [PublicAPI] public bool isSimple => !_isComplex;

    Ease _ease;
    [PublicAPI] public Ease ease => _ease ??= _isComplex ? _complex.ease : _simple.toEase();
    
    public void invalidate() {
      _ease = null;
      _complex?.invalidate();
    }

    public override string ToString() => 
      _isComplex 
        ? (_complex?.easeName ?? "not set") 
        : _simple.ToString();

    public string[] blacklistedFields() => 
      _isComplex
        ? new [] { nameof(_simple) }
        : new [] { nameof(_complex) };

    // TODO: implement instances
    interface IComplexSerializedEase {
      public string easeName { get; }
      public void invalidate();
      public Ease ease { get; }
    }
  }
}