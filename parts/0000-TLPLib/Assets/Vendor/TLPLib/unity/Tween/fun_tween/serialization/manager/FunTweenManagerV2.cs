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
#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized
      [SerializeField, PublicAccessor] float _startsAt;
      [SerializeField, HideInInspector] int _timelineChannelIdx;
      [SerializeField, NotNull, PublicAccessor, HideLabel, SerializeReference, InlineProperty] 
      ISerializedTweenTimelineElement _element;
      // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649
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
          foreach (var element in _elements) {
            var timelineElement = element.element.toTimelineElement();
            builder.insert(element.startsAt, timelineElement);
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
  }

  public interface ISerializedTweenTimelineElement {
    TweenTimelineElement toTimelineElement();
    Object getTarget();
    float duration { get; }
    void trySetDuration(float duration);
    
#if UNITY_EDITOR
    bool __editorDirty { get; }
    // string[] __editorSerializedProps { get; }
#endif
  }
}