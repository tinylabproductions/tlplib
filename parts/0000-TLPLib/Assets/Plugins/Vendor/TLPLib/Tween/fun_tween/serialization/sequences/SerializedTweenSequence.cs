using System;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// Serializable <see cref="TweenSequence"/>.
  /// </summary>
  [Serializable]
  public partial class SerializedTweenSequence {
    [Serializable]
    // Can't be struct, because AdvancedInspector freaks out.
    public partial class Element {
      enum At : byte { AfterLastElement, WithLastElement, SpecificTime }
      
      #region Unity Serialized Fields

#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
      [SerializeField, NotNull] string _title = "";
      [SerializeField] At _at;
      [SerializeField, Tooltip("in seconds"), Descriptor(nameof(timeOffsetDescription))] float _timeOffset;
      [SerializeField, NotNull, CreateDerived, PublicAccessor] SerializedTweenSequenceElement _element;
      // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

      #endregion

      Description timeOffsetDescription => new Description(
        _at == At.SpecificTime ? "Time" : "Time Offset"
      );

      public float at(float lastElementTime, float lastElementDuration) {
        switch (_at) {
          case At.AfterLastElement: return lastElementTime + lastElementDuration + _timeOffset;
          case At.WithLastElement: return lastElementTime + _timeOffset;
          case At.SpecificTime: return _timeOffset;
          default: throw new ArgumentOutOfRangeException(nameof(_at), _at.ToString(), "Unknown mode");
        }
      }

      public override string ToString() {
        var titleS = _title.isEmpty() ? "" : $"{_title} | ";
        var atS = 
          _at == At.SpecificTime
          ? $"@ {_timeOffset}s"
          : (
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            _timeOffset == 0 
            ? _at.ToString() 
            : _timeOffset > 0 
              ? $"{_at} + {_timeOffset}s"
              : $"{_at} - {-_timeOffset}s"
          );
        return $"{titleS}{atS}: {_element}";
      }
    }
    
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField, NotNull] Element[] _elements;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    TweenSequence _sequence;
    [PublicAPI]
    public TweenSequence sequence {
      get {
        if (_sequence == null) {
          var builder = new TweenSequence.Builder();
          var lastElementTime = 0f;
          var lastElementDuration = 0f;
          foreach (var element in _elements) {
            var currentElementTime =
              element.at(lastElementTime: lastElementTime, lastElementDuration: lastElementDuration);
            foreach (var elem in element.element.elements) {
              builder.insert(currentElementTime, elem);
            }
            lastElementTime = currentElementTime;
            lastElementDuration = element.element.duration;
          }
          _sequence = builder.build();
        }

        return _sequence;
      }
    }
  }
}