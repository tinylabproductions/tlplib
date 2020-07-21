using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using Sirenix.OdinInspector;
using UnityEngine;

using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager {
  public partial class FunTweenManagerV2 : MonoBehaviour, IMB_OnDestroy, IMB_Awake {
    [SerializeField] TweenTime _time = TweenTime.OnUpdate;
    [SerializeField] TweenManager.Loop _looping = TweenManager.Loop.single;
    [
      SerializeField, 
      HideLabel, 
      InlineProperty, 
      // timeline editor fails to update if we edit it from multiple places
      HideIf(nameof(timelineEditorIsOpen), animate: false)
    ] 
    SerializedTweenTimelineV2 _timeline = new SerializedTweenTimelineV2();

    public SerializedTweenTimelineV2 serializedTimeline => _timeline;
    public TweenTimeline timeline => _timeline.timeline;
    public string title => getGameObjectPath(transform);
    
    public static bool timelineEditorIsOpen;
    
    bool awakeCalled;
    TweenManager _manager;
    
    [LazyProperty] static ILog log => Log.d.withScope(nameof(FunTweenManagerV2));
    
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
          // if gameobject was never enabled, then OnDestroy will not be called :(
          var maybeParentComponent = (Application.isPlaying && !awakeCalled) ? Some.a<Component>(this) : None._;
          
          var tm = new TweenManager(
            _timeline.timeline, _time, _looping, context: gameObject, 
            maybeParentComponent: maybeParentComponent
          );
          
          if (maybeParentComponent.isSome) {
            log.mWarn(
              $"Trying to create tween manager while tween gameobject was not enabled. " +
              $"Using a workaround. Context: {tm.context}"
            );
          }
          
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
      if (!this) return;
      switch (action) {
        case Action.PlayForwards:                 manager.play(forwards: true);    break;
        case Action.PlayBackwards:                manager.play(forwards: false);   break;
        case Action.ResumeForwards:               manager.resume(forwards: true);  break;
        case Action.ResumeBackwards:              manager.resume(forwards: false); break;
        case Action.Resume:                       manager.resume();                break;
        case Action.Stop:                         manager.stop();                  break;
        case Action.Reverse:                      manager.reverse();               break;
        case Action.Rewind:                       manager.rewind(applyEffectsForRelativeTweens: false);     break;
        case Action.RewindWithEffectsForRelative: manager.rewind(applyEffectsForRelativeTweens: true);      break;
        case Action.ApplyZeroState:               manager.timeline.applyStateAt(0);                         break;
        case Action.ApplyMaxDurationState:        manager.timeline.applyStateAt(manager.timeline.duration); break;
        default: throw new ArgumentOutOfRangeException(nameof(action), action, null);
      }
    }

    public void OnDestroy() {
      _manager?.stop();
    }

    public void Awake() {
      awakeCalled = true;
    }
  }

  [Serializable]
  public partial class SerializedTweenTimelineV2 {
    [Serializable]
    public partial class Element {
      // Don't use nameof, because those fields exist only in UNITY_EDITOR
      const string CHANGE = "editorSetDirty";
      
#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized
      [SerializeField, PublicAccessor] float _startsAt;
      [SerializeField, HideInInspector] int _timelineChannelIdx;
      [SerializeField, NotNull, PublicAccessor, HideLabel, SerializeReference, InlineProperty, OnValueChanged(CHANGE)] 
      ISerializedTweenTimelineElementBase _element;
      // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649

      public bool isValid => _element?.isValid ?? false;
    }
    
    #region Unity Serialized Fields
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized
    [SerializeField, NotNull, OnValueChanged(nameof(invalidate))] Element[] _elements = new Element[0];
    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649
    #endregion

    TweenTimeline _timeline;
    [PublicAPI]
    public TweenTimeline timeline {
      get {
#if UNITY_EDITOR
        if (!Application.isPlaying && _timeline != null) {
          foreach (var element in _elements) {
            if (element.__editorDirty) {
              element.invalidate();
              _timeline = null;
            }
          }
        }
#endif
        if (_timeline == null) {
          var builder = new TweenTimeline.Builder();
          foreach (var element in _elements) {
            if (element.isValid) {
              var timelineElement = element.element.toTimelineElement();
              builder.insert(element.startsAt, timelineElement);
            }
            else if (Application.isPlaying) {
              // TODO: add context
              Log.d.error("Element in animation is invalid. Skipping broken element.", this);
            }
          }
          _timeline = builder.build();
#if UNITY_EDITOR
          if (!Application.isPlaying) {
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
          }
#endif
        }

        return _timeline;
      }
    }

    public void invalidate() => _timeline = null;
  }
  
  public interface ISerializedTweenTimelineElementBase {
    TweenTimelineElement toTimelineElement();
    float duration { get; }
    void trySetDuration(float duration);
    Object getTarget();
    bool isValid { get; }

#if UNITY_EDITOR
    bool __editorDirty { get; }
    // string[] __editorSerializedProps { get; }
#endif
  }
  
  public interface ISerializedTweenTimelineCallback : ISerializedTweenTimelineElementBase {
    
  }

  public interface ISerializedTweenTimelineElement : ISerializedTweenTimelineElementBase {
  }
}