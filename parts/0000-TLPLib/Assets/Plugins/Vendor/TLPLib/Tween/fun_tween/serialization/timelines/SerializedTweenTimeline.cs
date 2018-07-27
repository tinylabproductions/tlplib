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
  /// Serializable <see cref="TweenTimeline"/>.
  /// </summary>
  [Serializable]
  public partial class SerializedTweenTimeline : Invalidatable {
    [Serializable]
    // Can't be struct, because AdvancedInspector freaks out.
    public partial class Element : Invalidatable {
      public enum At : byte { AfterLastElement, WithLastElement, SpecificTime }
      
      #region Unity Serialized Fields

#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
      [SerializeField, NotNull] string _title = "";
      [SerializeField] At _startAt;
      [SerializeField, Tooltip("in seconds"), Descriptor(nameof(timeOffsetDescription))] float _timeOffset;
      [SerializeField, NotNull, CreateDerived, PublicAccessor] SerializedTweenTimelineElement _element;
      // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

      #endregion
      
#if UNITY_EDITOR
      [SerializeField, HideInInspector] int _timelineChannelIdx;
      
      public int timelineChannelIdx {
        get => _timelineChannelIdx;
        set => _timelineChannelIdx = value;
      }
      
      public string title {
        get => _title;
        set => _title = value;
      }

      public At startAt {
        get => _startAt;
        set => _startAt = value;
      }

      public float timeOffset {
        get => _timeOffset;
        set => _timeOffset = value;
      }
#endif

      public void invalidate() => _element.invalidate();

      Description timeOffsetDescription => new Description(
        _startAt == At.SpecificTime ? "Time" : "Time Offset"
      );

      public float at(float lastElementTime, float lastElementDuration) {
        switch (_startAt) {
          case At.AfterLastElement: return lastElementTime + lastElementDuration + _timeOffset;
          case At.WithLastElement: return lastElementTime + _timeOffset;
          case At.SpecificTime: return _timeOffset;
          default: throw new ArgumentOutOfRangeException(nameof(_startAt), _startAt.ToString(), "Unknown mode");
        }
      }

      public override string ToString() {
        var titleS = _title.isEmpty() ? "" : $"{_title} | ";
        var atS = 
          _startAt == At.SpecificTime
          ? $"@ {_timeOffset}s"
          : (
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            _timeOffset == 0 
            ? _startAt.ToString() 
            : _timeOffset > 0 
              ? $"{_startAt} + {_timeOffset}s"
              : $"{_startAt} - {-_timeOffset}s"
          );
        return $"{titleS}{atS} {_element}";
      }
    }
    
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField, NotNull] Element[] _elements;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    TweenTimeline _timeline;
    [PublicAPI]
    public TweenTimeline timeline {
      get {
        if (_timeline == null) {
          var builder = new TweenTimeline.Builder();
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
          _timeline = builder.build();
        }

        return _timeline;
      }
    }
    
#if UNITY_EDITOR
    public Element[] elements {
      get => _elements;
      set => _elements = value;
    }
#endif
    
    public void invalidate() {
      _timeline = null;
      foreach (var element in _elements)
        element.invalidate();
    }
  }
}