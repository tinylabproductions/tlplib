using System;
using com.tinylabproductions.TLPLib.attributes;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using Sirenix.OdinInspector;
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
      enum At : byte { AfterLastElement, WithLastElement, SpecificTime }
      
      #region Unity Serialized Fields

#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
      [SerializeField, NotNull] string _title = "";
      [SerializeField] At _at;
      [
        SerializeField, Tooltip("in seconds"), LabelText("$" + nameof(timeOffsetDescription))
      ] float _timeOffset;
      [SerializeField, NotNull, PublicAccessor, HideLabel] SerializedTweenTimelineElement _element;
      // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

      #endregion

      public void invalidate() => _element.invalidate();

      string timeOffsetDescription => _at == At.SpecificTime ? "Time" : "Time Offset";

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

    public void invalidate() {
      _timeline = null;
      foreach (var element in _elements)
        element.invalidate();
    }
    
#if UNITY_EDITOR
    [ShowInInspector, PropertyRange(0, nameof(duration)), PropertyOrder(-1)] float __setProgress {
      get {
        try {
          return timeline.timePassed;
        }
        catch (Exception) {
          return 0;
        }
      }
      set => timeline.timePassed = value;
    }

    float duration {
      get {
        try {
          return timeline.duration;
        }
        catch (Exception) {
          return 0;
        }
      }
    }
#endif
  }
}