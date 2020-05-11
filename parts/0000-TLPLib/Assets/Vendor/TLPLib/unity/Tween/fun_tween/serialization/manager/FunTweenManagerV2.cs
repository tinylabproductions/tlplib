using System;
using System.Collections.Generic;
using GenerationAttributes;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager {
  public partial class FunTweenManagerV2 : MonoBehaviour {
    [SerializeField] TweenTime _time = TweenTime.OnUpdate;
    [SerializeField] TweenManager.Loop _looping = TweenManager.Loop.single;
    [SerializeField, HideLabel, InlineProperty] SerializedTweenTimelineV2 _timeline;

    // [OnInspectorGUI(nameof(ttt))]
    // public string sss;
    //
    // void ttt() {
    //   foreach (var style in GUI.skin.customStyles) {
    //     if (style.name.ToLower().Contains(sss)) GUILayout.Button(style.name, style);
    //   }
    // }

    public SerializedTweenTimelineV2 serializedTimeline => _timeline;
    public TweenTimeline timeline => _timeline.timeline;

    TweenManager _manager;

    public string title => getGameObjectPath(transform);
    
    static string getGameObjectPath(Transform transform) {
      var path = transform.gameObject.name;
      while (transform.parent != null) {
        transform = transform.parent;
        path = path + " < " + transform.gameObject.name;
      }
      return path;
    }

    [PublicAPI]
    public TweenManager manager {
      get {
        TweenManager create() {
          var tm = new TweenManager(_timeline.timeline, _time, _looping, context: gameObject);
          return tm;
        }

        return _manager ??= create();
      }
    }

    public void recreate() {
      _manager = null;
      _timeline.invalidate();
    }
    
    public enum Action : byte {
      PlayForwards = 0, 
      PlayBackwards = 1, 
      ResumeForwards = 2, 
      ResumeBackwards = 3, 
      Resume = 4, 
      Stop = 5, 
      Reverse = 6, 
      Rewind = 7, 
      RewindWithEffectsForRelative = 8,
      ApplyZeroState = 9,
      ApplyMaxDurationState = 10
    }

    public void run(Action action) {
      // TODO: implement
    }
  }

  [Serializable]
  public partial class SerializedTweenTimelineV2 {
    [Serializable]
    public partial class Element {
      public enum At : byte { AfterLastElement, WithLastElement, SpecificTime }
      
#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized
      [SerializeField, HorizontalGroup(Width = 140), HideLabel] At _at;
      
      [
        SerializeField, HorizontalGroup(MarginLeft = 10), LabelWidth(80), Tooltip("in seconds"), 
        LabelText("$" + nameof(timeOffsetLabel)),
      ] 
      float _timeOffset;
      [SerializeField, NotNull, PublicAccessor, HideLabel, SerializeReference, InlineProperty] 
      ISerializedTweenTimelineElement _element;
      // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649

      string timeOffsetLabel => _at == At.SpecificTime ? "Time" : "Time Offset";

      public float at(float lastElementTime, float lastElementDuration) {
        return _at switch {
          At.AfterLastElement => lastElementTime + lastElementDuration + _timeOffset,
          At.WithLastElement => lastElementTime + _timeOffset,
          At.SpecificTime => _timeOffset,
          _ => throw new ArgumentOutOfRangeException(nameof(_at), _at.ToString(), "Unknown mode")
        };
      }
    }
    
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized
    [SerializeField, NotNull, OnValueChanged(nameof(invalidate))] Element[] _elements;
    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649

    #endregion

    TweenTimeline _timeline;
    [PublicAPI]
    public TweenTimeline timeline {
      get {
#if UNITY_EDITOR
        if (_timeline != null) {
          foreach (var element in _elements) {
            if (element.element.__editorDirty) {
              element.invalidate();
              _timeline = null;
            }
          }
        }
#endif
        if (_timeline == null) {
          var builder = new TweenTimeline.Builder();
          var lastElementTime = 0f;
          var lastElementDuration = 0f;
          foreach (var element in _elements) {
            var currentElementTime =
              element.at(lastElementTime: lastElementTime, lastElementDuration: lastElementDuration);
            var timelineElement = element.element.toTimelineElement();
            builder.insert(currentElementTime, timelineElement);
            lastElementTime = currentElementTime;
            lastElementDuration = timelineElement.duration;
          }
          _timeline = builder.build();
#if UNITY_EDITOR
          // restore cached position
          _timeline.timePassed = __editor_cachedTimePassed;

          {
            // find all key frames
            var keyframes = new List<float>();
            keyframes.Add(_timeline.duration);
            foreach (var e in _timeline.effects) {
              keyframes.Add(e.startsAt);
              keyframes.Add(e.endsAt);
            }
            keyframes.Sort();
            var filtered = __editor_keyframes;
            filtered.Clear();
            filtered.Add(0);
            foreach (var keyframe in keyframes) {
              if (!Mathf.Approximately(filtered[filtered.Count - 1], keyframe)) {
                filtered.Add(keyframe);
              }
            }
          }
#endif
        }

        return _timeline;
      }
    }

    public void invalidate() => _timeline = null;
    
    [ShowInInspector] public static bool editorDisplayEndAsDelta;
    [ShowInInspector] public static bool editorDisplayCurrent = true;
    [ShowInInspector] public static bool editorDisplayEasePreview = true;

#if UNITY_EDITOR
    [ShowInInspector, PropertyRange(0, nameof(__editor_duration)), PropertyOrder(-2), LabelText("Set Progress"), LabelWidth(100)] 
    float __editor_progress {
      get { try { return timeline.timePassed; } catch (Exception _) { return default; } }
      set {
        timeline.timePassed = value;
        __editor_cachedTimePassed = value;
      }
    }
    
    [ShowInInspector, PropertyRange(0, nameof(__editor_keyFrameCount)), PropertyOrder(-1), LabelText("Keyframes"), LabelWidth(100)] 
    int __editor_setProgressKeyframes {
      get {
        var closest = 0;
        var dist = float.PositiveInfinity;
        var progress = __editor_progress;
        for (var i = 0; i < __editor_keyframes.Count; i++) {
          var newDist = Math.Abs(__editor_keyframes[i] - progress);
          if (newDist < dist) {
            dist = newDist;
            closest = i;
          }
        }
        return closest;
      }
      set {
        if (value < __editor_keyframes.Count) __editor_progress = __editor_keyframes[value];
      }
    }
    
    float __editor_duration {
      get { try { return timeline.duration; } catch (Exception _) { return 0; } }
    }
    
    List<float> __editor_keyframes = new List<float>();
    int __editor_keyFrameCount => __editor_keyframes.Count - 1;

    float __editor_cachedTimePassed;
#endif
  }

  public interface ISerializedTweenTimelineElement {
    TweenTimelineElement toTimelineElement();
    Object getTarget();
    float duration { get; }
    void trySetDuration(float duration);
    
#if UNITY_EDITOR
    bool __editorDirty { get; }
    string[] __editorSerializedProps { get; }
#endif
  }
}