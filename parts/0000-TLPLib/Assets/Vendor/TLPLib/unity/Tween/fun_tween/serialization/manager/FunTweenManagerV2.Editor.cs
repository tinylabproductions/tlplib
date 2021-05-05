using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using pzd.lib.exts;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager {
  public partial class FunTweenManagerV2 {
  }

  public partial class SerializedTweenTimelineV2 {
    public partial class Element {
      public float setStartsAt(float value) => _startsAt = value;
      
      string _title;
      public string title { get {
        if (_title.isNullOrEmpty()) _title = generateTitle();
        return _title;
      } }

      string generateTitle() {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (_element == null) return "NULL";
        var target = _element.getTarget();
        if (target is Component c && c) target = c.gameObject;
        return $"{(target ? target.name : "NULL")} : {ObjectNames.NicifyVariableName(_element.GetType().Name)}";
      }

      public int timelineChannelIdx {
        get => _timelineChannelIdx;
        set => _timelineChannelIdx = value;
      }

      public void invalidate() {
        _title = null;
        ___editorDirty = false;
      }
      
      bool ___editorDirty = true;
      public bool __editorDirty => ___editorDirty || (_element?.__editorDirty ?? false);
      [UsedImplicitly] void editorSetDirty() => ___editorDirty = true;
    }
    
    [ShowInInspector] public static bool editorDisplayEndAsDelta;
    [ShowInInspector] public static bool editorDisplayCurrent = true;
    [ShowInInspector] public static bool editorDisplayEasePreview = true;
    
    public Element[] elements {
      get => _elements;
      set => _elements = value;
    }
    
    [ShowInInspector, PropertyRange(0, nameof(__editor_duration)), PropertyOrder(-2), LabelText("Set Progress"), LabelWidth(100)] 
    float __editor_progress {
      get { try { return timeline().timePassed; } catch (Exception) { return default; } }
      set {
        timeline().timePassed = value;
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
      get { try { return timeline().duration; } catch (Exception) { return 0; } }
    }
    
    List<float> __editor_keyframes = new List<float>();
    int __editor_keyFrameCount => __editor_keyframes.Count - 1;

    float __editor_cachedTimePassed;
  }
}

#endif