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
  public partial class FunTweenManagerV2 : MonoBehaviour, IMB_OnDestroy, IMB_Awake, IMB_OnEnable, IMB_OnDisable {
    enum AutoPlayMode {
      None = 0,
      OnEnable = 1
    }
    
    [SerializeField] TweenTime _time = TweenTime.OnUpdate;
    [SerializeField] TweenManager.Loop _looping = TweenManager.Loop.single;
    [SerializeField] AutoPlayMode _autoPlayMode = AutoPlayMode.None;
    [
      SerializeField, 
      HideLabel, 
      InlineProperty, 
      // timeline editor fails to update if we edit it from multiple places
      HideIf(nameof(timelineEditorIsOpen), animate: false)
    ] 
    SerializedTweenTimelineV2 _timeline = new();

    public SerializedTweenTimelineV2 serializedTimeline => _timeline;
    public TweenTimeline timeline => _timeline.timeline(this);
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
          // if game object was never enabled, then OnDestroy will not be called :(
          var maybeParentComponent = Application.isPlaying && !awakeCalled ? Some.a<Component>(this) : None._;
          
          var tm = new TweenManager(
            _timeline.timeline(this),
            // We manage the lifetime manually.
            TweenManagerLifetime.unbounded, 
            _time, _looping, context: gameObject,
            maybeParentComponent: maybeParentComponent
          );
          
          // Disabling this because it seems that nobody cares about this warning anyway and it runs just fine with
          // the workaround. -- Artūras Šlajus (2021-01-09)
          //
          // if (maybeParentComponent.isSome) {
          //   log.mWarn(
          //     $"Trying to create tween manager while tween game object was not enabled. " +
          //     $"Using a workaround. Context: {tm.context}"
          //   );
          // }
          
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
      ApplyMaxDurationState = 10,
      StopAndRewind = 11
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
        case Action.StopAndRewind:                manager.stop(); manager.rewind(); break;
        default: throw new ArgumentOutOfRangeException(nameof(action), action, null);
      }
    }

    public void OnDestroy() => _manager?.stop();

    public void Awake() => awakeCalled = true;

    public void OnEnable() {
      if (_autoPlayMode == AutoPlayMode.OnEnable) {
        manager.play();
      }
    }

    public void OnDisable() {
      if (_autoPlayMode == AutoPlayMode.OnEnable) {
        manager.stop();
      }
    }
  }

  [Serializable]
  public partial class SerializedTweenTimelineV2 {
    [Serializable]
    public partial class Element {
      // Don't use nameof, because those fields exist only in UNITY_EDITOR
      const string CHANGE = "editorSetDirty";
      const string TIME = "Time";
      
#pragma warning disable 649
      // ReSharper disable NotNullMemberIsNotInitialized
      [SerializeField, PublicAccessor, HorizontalGroup(TIME)] float _startsAt;
      [SerializeField, HideInInspector] int _timelineChannelIdx;
      [
        NotNull, PublicAccessor, HideLabel, SerializeReference, InlineProperty, OnValueChanged(CHANGE),
        HideReferenceObjectPicker, OnInspectorGUI(nameof(drawType), append: false)
      ] 
      ISerializedTweenTimelineElementBase _element;
      // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649
      
      public Element() {}
      
      public Element(float startsAt, int timelineChannelIdx, ISerializedTweenTimelineElementBase element) {
        _startsAt = startsAt;
        _timelineChannelIdx = timelineChannelIdx;
        _element = element;
      }

      partial void drawType();

      [ShowInInspector, HorizontalGroup(TIME)] float _endTime {
        get => _startsAt + _element?.duration ?? 0f;
        set {
          if (_element != null) {
            _element.trySetDuration(value - startsAt);
          }
        }
      }

      public bool isValid => _element?.isValid ?? false;

      public Element deepClone() {
        // Element is Unity serializable object. JsonUtility serialization should always work here.
        var json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<Element>(json);
      }
    }
    
    #region Unity Serialized Fields
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized
    [SerializeField, NotNull, OnValueChanged(nameof(invalidate))] Element[] _elements = new Element[0];
    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649
    #endregion

    TweenTimeline _timeline;
    public bool buildingTimeline { get; private set; }
    [PublicAPI]
    public TweenTimeline timeline(Object parent = null) {
      if (buildingTimeline) {
        logError("Cyclical timeline building was detected.");
        return TweenTimeline.builder().build();
      }
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
        buildingTimeline = true;
        try {
          _timeline = buildTimeline();
          timelineWasBuilt(_timeline);
        }
        finally {
          buildingTimeline = false;
        }
      }

      return _timeline;
      
      TweenTimeline buildTimeline() {
        var builder = new TweenTimeline.Builder();
        foreach (var element in _elements) {
          if (validateElement()) {
            var timelineElement = element.element.toTimelineElement();
            builder.insert(element.startsAt, timelineElement);
          }
          else if (Application.isPlaying) {
            logError("Element in animation is invalid. Skipping broken element.");
          }

          bool validateElement() {
            if (!element.isValid) return false;
            {if (element.element is tweeners.TweenManager tmTween) {
              if (tmTween.target.serializedTimeline.buildingTimeline) {
                return false;
              }
            }}
            return true;
          }
        }
        return builder.build();
      }
      
      void logError(string message) {
        Log.d.error(
          $"{message} Parent={(parent != null ? parent.name : "none")}",
          context: (object) parent ?? this
        );
      }
    }

    void timelineWasBuilt(TweenTimeline timeline) {
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        // restore cached position
        timeline.timePassed = __editor_cachedTimePassed;
        {
          // find all key frames
          var keyframes = new List<float>();
          keyframes.Add(timeline.duration);
          foreach (var e in timeline.effects) {
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

    public void invalidate() => _timeline = null;
  }

  public interface ISerializedTweenTimelineElementBase {
    TweenTimelineElement toTimelineElement();
    float duration { get; }
    void trySetDuration(float duration);
    Object getTarget();
    bool isValid { get; }
    Color editorColor { get; }

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