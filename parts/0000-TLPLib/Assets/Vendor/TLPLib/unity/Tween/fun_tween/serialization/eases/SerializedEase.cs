using System;
using com.tinylabproductions.TLPLib.attributes;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  [Serializable, InlineProperty]
  public partial class SerializedEase : ISkipObjectValidationFields, Invalidatable {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField, HideLabel, HorizontalGroup, OnValueChanged(nameof(complexChanged)), PublicAccessor] bool _isComplex;
    [SerializeField, HideLabel, HorizontalGroup, ShowIf(nameof(isSimple))] SimpleSerializedEase _simple;
    [SerializeField, HideLabel, HorizontalGroup, ShowIf(nameof(isComplex)), TLPCreateDerived, NotNull] ComplexSerializedEase _complex;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion
    
    [HideLabel, HorizontalGroup, ShowIf(nameof(isSimple)), ShowInInspector, PreviewField]
    Texture2D _simplePreview {
      get {
        const float verticalOffset = 0.25f;
        var texture = new Texture2D(64, 64);
        texture.fill(Color.black);
        for (var _x = 0; _x < texture.width; _x++) {
          var _y = (int) (
            _simple.toEase().Invoke(_x / (float) texture.width)
            * texture.height
            * (1f - verticalOffset * 2f) 
            + verticalOffset * texture.height
          );
          
          texture.SetPixel(_x, (int) (verticalOffset * texture.height), Color.gray);
          texture.SetPixel(_x, (int) ((1f - verticalOffset) * texture.height), Color.gray);
          
          // 2x2 pixel
          for (var x = _x; x < _x + 2 && x < texture.width; x++) {
            for (var y = _y; y < _y + 2 && y < texture.height; y++) {
              texture.SetPixel(x, y, Color.green);
            }
          }
        }
        texture.Apply();
        return texture;
      }
    }

    [PublicAPI] public SerializedEase(SimpleSerializedEase simple) {
      _isComplex = false;
      _simple = simple;
    }

    [PublicAPI] public SerializedEase(ComplexSerializedEase complex) {
      _isComplex = true;
      _complex = complex;
    }
    
    void complexChanged() {
      // ReSharper disable AssignNullToNotNullAttribute
      if (isSimple) _complex = default;
      // ReSharper restore AssignNullToNotNullAttribute
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