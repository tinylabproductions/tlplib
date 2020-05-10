#if ADV_INS_CHANGES && UNITY_EDITOR  && FALSE //TODO
using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
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

    public TweenPlaybackController(FunTweenManager ftm, Ref<bool> visualizationMode) {
      manager = ftm;
      this.visualizationMode = visualizationMode;
      beforeCursorDataIsSaved = false;
    }

    readonly FunTweenManager manager;
    double lastSeconds;

    bool isAnimationPlaying, applicationPlaying, playingBackwards, beforeCursorDataIsSaved;
    readonly Ref<bool> visualizationMode;
    Option<EditorApplication.CallbackFunction> updateDelegateOpt;
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
      
      manager.timeline.timeline.timePassed += timeDiff * (playingBackwards ? -1 : 1);
    }

    void startUpdatingTime() {
      if (!EditorApplication.isCompiling) {
        startVisualization();
        lastSeconds = currentRealSeconds();
        updateDelegateOpt.voidFold(
          () => {
            EditorApplication.CallbackFunction updateDelegate = updateAnimation;
            EditorApplication.update += updateDelegate;
            updateDelegateOpt = updateDelegate.some();
          },
          updateDelegate => EditorApplication.update += updateDelegate
        );
      }
    }
  
    void stopTimeUpdate() =>
      updateDelegateOpt.map(updateDelegate => EditorApplication.update -= updateDelegate);
  
    void startVisualization() {
      if (!visualizationMode.value && getTimelineTargets(manager).valueOut(out var data)) {
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
    
    static Option<Object[]> getTimelineTargets(FunTweenManager ftm) =>
      ftm.timeline.elements.opt().map( elements =>
        elements
          .map(elem => elem.element.getTargets())
          .Aggregate((acc, curr) => acc.concat(curr))
      );

    public void evaluateCursor(float time) {
      if (getTimelineTargets(manager).valueOut(out var data) && data.All(target => target != null)) {

        if (!Application.isPlaying) {
          if (visualizationMode.value) {
            stopTimeUpdate();
          }
          else if (!beforeCursorDataIsSaved) {
            manager.recreate();
            beforeCursorDataIsSaved = true;
            Undo.RegisterCompleteObjectUndo(data, "targets saved");
            savedTargetDataOpt = data.some();
          }
        }
        else {
            manager.run(FunTweenManager.Action.Stop);
        }
        
        foreach (var entry in data) {
          EditorUtility.SetDirty(entry);
        }

        manager.timeline.timeline.timePassed = time;

      }
      else {
        Log.d.warn($"Set targets before evaluating!");
      }
    }

    public void stopCursorEvaluation() {
      if (!visualizationMode.value && beforeCursorDataIsSaved && !Application.isPlaying) {
        Undo.RevertAllInCurrentGroup();
        savedTargetDataOpt = F.none_;
        beforeCursorDataIsSaved = false;
      }
    }
    
    public void manageAnimation(AnimationPlaybackEvent playbackEvent) {
      applicationPlaying = Application.isPlaying;
      switch (playbackEvent) {
        case AnimationPlaybackEvent.GoToStart:
          manager.timeline.timeline.timePassed = 0;
          if (!applicationPlaying) {
            stopTimeUpdate();
            startVisualization();
          }
          else {
            manager.run(FunTweenManager.Action.Stop);
          }
          isAnimationPlaying = false;
          break;
        
        case AnimationPlaybackEvent.PlayFromStart:
          if (!applicationPlaying) {
            if (!isAnimationPlaying) {
              startUpdatingTime();
            }
            
            manager.timeline.timeline.timePassed = 0;
  
            if (!visualizationMode.value) {
              startVisualization();
            }
            playingBackwards = false;
          }
          else {
            manager.run(FunTweenManager.Action.PlayForwards);
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
              manager.run(FunTweenManager.Action.Stop);
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
              manager.run(FunTweenManager.Action.Resume);
            }

            isAnimationPlaying = true;
          }

          break;
        
        case AnimationPlaybackEvent.Pause:
          if (!applicationPlaying){
            stopTimeUpdate();
          }
          else {
            manager.run(FunTweenManager.Action.Stop);
          }
          isAnimationPlaying = false;
          break;
        
        case AnimationPlaybackEvent.PlayFromEnd:
          if (!applicationPlaying) {
            if (!visualizationMode.value)  { startVisualization(); }
            manager.timeline.timeline.timePassed = manager.timeline.timeline.duration;
            if (!isAnimationPlaying) { startUpdatingTime();  }
            playingBackwards = true;
          }
          else {
            manager.run(FunTweenManager.Action.PlayBackwards);
          }
          isAnimationPlaying = true;
          break;
        
        case AnimationPlaybackEvent.GoToEnd:
          if (!applicationPlaying) {
            if (!visualizationMode.value) {
              startVisualization();
            }
            manager.timeline.timeline.timePassed = manager.timeline.timeline.duration;
            stopTimeUpdate();
          }
          else {
            manager.timeline.timeline.timePassed = manager.timeline.timeline.duration;
            manager.run(FunTweenManager.Action.Stop);
          }
          isAnimationPlaying = false;
          break;
        
        case AnimationPlaybackEvent.Reverse:
          if (!applicationPlaying) {
            playingBackwards = !playingBackwards;
          }
          else {
            manager.run(FunTweenManager.Action.Reverse);
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