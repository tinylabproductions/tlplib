#if UNITY_EDITOR
using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using pzd.lib.functional;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public class TweenPlaybackController {
    
    public enum AnimationPlaybackEvent : byte {
      GoToStart,
      PlayFromStart,
      PlayFromCurrentTime,
      Pause,
      PlayFromEnd,
      GoToEnd,
      Reverse,
      Exit
    }

    public TweenPlaybackController(FunTweenManagerV2 ftm, Ref<bool> visualizationMode) {
      manager = ftm;
      this.visualizationMode = visualizationMode;
      beforeCursorDataIsSaved = false;
      updateDelegate = updateAnimation;
    }

    readonly FunTweenManagerV2 manager;
    double lastSeconds;

    bool isAnimationPlaying, applicationPlaying, playingBackwards, beforeCursorDataIsSaved;
    readonly Ref<bool> visualizationMode;
    readonly EditorApplication.CallbackFunction updateDelegate;
    Option<Object[]> savedTargetDataOpt;

    static double currentRealSeconds() {
      var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      return (DateTime.UtcNow - epochStart).TotalSeconds;
    }

    void updateAnimation() {
      var currentSeconds = currentRealSeconds();
      if (lastSeconds < 1) lastSeconds = currentSeconds;
      var timeDiff = (float)(currentSeconds - lastSeconds);
      lastSeconds = currentSeconds;
      
      foreach (var savedTargetData in savedTargetDataOpt) {
        foreach (var entry in savedTargetData) {
          EditorUtility.SetDirty(entry);
        }
      }

      var previous = manager.timeline.timePassed;
      var updated = previous + timeDiff * (playingBackwards ? -1 : 1);
      manager.timeline.timePassed = updated;
      // if (updated < 0 || updated >= manager.timeline.duration) stopVisualization();
    }

    void startUpdatingTime() {
      if (!EditorApplication.isCompiling) {
        startVisualization();
        lastSeconds = currentRealSeconds();
        EditorApplication.update -= updateDelegate;
        EditorApplication.update += updateDelegate;
      }
    }
  
    void stopTimeUpdate() => EditorApplication.update -= updateDelegate;
  
    void startVisualization() {
      if (!visualizationMode.value) {
        var data = getValidTimelineTargets(manager);
        manager.recreate();
        Undo.RegisterCompleteObjectUndo(data, "Animating targets");
        savedTargetDataOpt = data.some();
        visualizationMode.value = true;
      }
    }
  
    public void stopVisualization() {
      if (visualizationMode.value) {
        stopTimeUpdate();
        Undo.PerformUndo();
        savedTargetDataOpt = F.none_;
        visualizationMode.value = false;
      }
    }
    
    static Object[] getValidTimelineTargets(FunTweenManagerV2 ftm) =>
      ftm.serializedTimeline.elements.Where(_ => _.isValid).Select(elem => elem.element.getTarget()).ToArray();
    
    static string getPath(Transform transform) {
      var path = transform.gameObject.name;
      while (transform.parent != null) {
        transform = transform.parent;
        path = transform.gameObject.name + "/" + path;
      }
      return path;
    }
    
    static EditorCurveBinding curve = EditorCurveBinding.FloatCurve("", typeof(object), "");

    public void evaluateCursor(float time) {
      var data = getValidTimelineTargets(manager);
      if (data.All(target => target != null)) {
        if (!Application.isPlaying) {
          if (visualizationMode.value) {
            stopTimeUpdate();
          }
          else if (!beforeCursorDataIsSaved) {
            manager.recreate();
            beforeCursorDataIsSaved = true;
            Undo.RegisterCompleteObjectUndo(data, "targets saved");
            savedTargetDataOpt = data.some();
            
            // TODO: implement this properly later
            // AnimationMode.StartAnimationMode();
            // foreach (var element in manager.serializedTimeline.elements) {
            //   foreach (var prop in element.element.__editorSerializedProps) {
            //     var pm = new PropertyModification() {
            //       target = element.element.getTarget(),
            //       propertyPath = prop,
            //       value = new SerializedObject(element.element.getTarget()).FindProperty(prop)
            //         .floatValue.ToString(CultureInfo.InvariantCulture)
            //     };
            //     AnimationMode.AddPropertyModification(curve, pm, true);
            //   }
            // }
          }
        }
        else {
            manager.run(FunTweenManagerV2.Action.Stop);
        }
        
        EditorUtility.SetDirty(manager);

        manager.timeline.timePassed = time;

      }
      else {
        Log.d.warn($"Set targets before evaluating!");
      }
    }

    public void stopCursorEvaluation() {
      if (!visualizationMode.value && beforeCursorDataIsSaved && !Application.isPlaying) {
        Undo.RevertAllInCurrentGroup();
        // AnimationMode.StopAnimationMode();
        savedTargetDataOpt = F.none_;
        beforeCursorDataIsSaved = false;
      }
    }
    
    public void manageAnimation(AnimationPlaybackEvent playbackEvent) {
      applicationPlaying = Application.isPlaying;
      switch (playbackEvent) {
        case AnimationPlaybackEvent.GoToStart:
          manager.timeline.timePassed = 0;
          if (!applicationPlaying) {
            stopTimeUpdate();
            startVisualization();
          }
          else {
            manager.run(FunTweenManagerV2.Action.Stop);
          }
          isAnimationPlaying = false;
          break;
        
        case AnimationPlaybackEvent.PlayFromStart:
          if (!applicationPlaying) {
            if (!isAnimationPlaying) {
              startUpdatingTime();
            }
            
            manager.timeline.timePassed = 0;
  
            if (!visualizationMode.value) {
              startVisualization();
            }
            playingBackwards = false;
          }
          else {
            manager.run(FunTweenManagerV2.Action.PlayForwards);
          }
          isAnimationPlaying = true;
          break;
        
        case AnimationPlaybackEvent.PlayFromCurrentTime:
          if (isAnimationPlaying) {
            if (!applicationPlaying) {
              stopTimeUpdate();
              playingBackwards = false;
            }
            else {
              manager.run(FunTweenManagerV2.Action.Stop);
            }
            isAnimationPlaying = false;
          }
          else {
            if (!applicationPlaying) {

              if (!visualizationMode.value) {
                startVisualization();
              }

              startUpdatingTime();
              playingBackwards = false;
            }
            else {
              manager.run(FunTweenManagerV2.Action.Resume);
            }

            isAnimationPlaying = true;
          }

          break;
        
        case AnimationPlaybackEvent.Pause:
          if (!applicationPlaying){
            stopTimeUpdate();
          }
          else {
            manager.run(FunTweenManagerV2.Action.Stop);
          }
          isAnimationPlaying = false;
          break;
        
        case AnimationPlaybackEvent.PlayFromEnd:
          if (!applicationPlaying) {
            if (!visualizationMode.value)  { startVisualization(); }
            manager.timeline.timePassed = manager.timeline.duration;
            if (!isAnimationPlaying) { startUpdatingTime();  }
            playingBackwards = true;
          }
          else {
            manager.run(FunTweenManagerV2.Action.PlayBackwards);
          }
          isAnimationPlaying = true;
          break;
        
        case AnimationPlaybackEvent.GoToEnd:
          if (!applicationPlaying) {
            if (!visualizationMode.value) {
              startVisualization();
            }
            manager.timeline.timePassed = manager.timeline.duration;
            stopTimeUpdate();
          }
          else {
            manager.timeline.timePassed = manager.timeline.duration;
            manager.run(FunTweenManagerV2.Action.Stop);
          }
          isAnimationPlaying = false;
          break;
        
        case AnimationPlaybackEvent.Reverse:
          if (!applicationPlaying) {
            playingBackwards = !playingBackwards;
          }
          else {
            manager.run(FunTweenManagerV2.Action.Reverse);
          }
          break;
        
        case AnimationPlaybackEvent.Exit:
          stopVisualization();
          isAnimationPlaying = false;
          break;
      }
    }
  }
}
#endif