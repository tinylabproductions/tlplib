using System;
using com.tinylabproductions.TLPLib.attributes;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager {
  /// <summary>
  /// Serialized <see cref="TweenManager"/>.
  /// </summary>
  public class FunTweenManager : MonoBehaviour, IMB_Start, IMB_OnEnable, IMB_OnDisable, IMB_OnDestroy, Invalidatable {
    const string TAB_FIELDS = "Fields", TAB_ACTIONS = "Actions";
    // ReSharper disable once UnusedMember.Local
    enum RunMode { Local, Global }
    // ReSharper disable once UnusedMember.Local
    enum AutoplayMode {
      Disabled = 0, Enabled = 1, ApplyZeroStateOnStart = 2, ApplyEndStateOnStart = 3, EnabledAndApplyZeroStateOnStart = 4
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS), UsedImplicitly, ReadOnly]
    float timePassed => _manager == null ? -1 : _manager.timeline.timePassed;
    [ShowInInspector, TabGroup(TAB_ACTIONS), UsedImplicitly, ReadOnly]
    uint currentIteration => _manager == null ? 0 : _manager.currentIteration;
    [ShowInInspector, TabGroup(TAB_ACTIONS), UsedImplicitly, ReadOnly]
    float timescale => _manager == null ? -1 : _manager.timescale;

    #region Unity Serialized Fields
#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [
      SerializeField, TabGroup(TAB_FIELDS),
      InfoBox(
        infoMessageType: InfoMessageType.Info, 
        message: 
          "Local mode pauses tweens when this game object is disabled and resumes when it is enabled.\n" +
          "Global mode continues to run the tween even if irrespective of this game objects state."
      )
    ] RunMode _runMode = RunMode.Local;
    [
      SerializeField, TabGroup(TAB_FIELDS),
      InfoBox(
        infoMessageType: InfoMessageType.Info,
        message: 
          "Modes:\n" +
          "- " + nameof(AutoplayMode.Disabled) + ": does nothing.\n" +
          "- " + nameof(AutoplayMode.Enabled) + ": starts playing forwards upon enabling this game object. Pauses upon " +
          "disabling game object, resumes when it is enabled again.\n" +
          "- " + nameof(AutoplayMode.ApplyZeroStateOnStart) + ": when this script receives Unity Start callback, " +
          "sets all the properties of non-relatively tweened objects like it was zeroth second in the timeline.\n" +
          "- " + nameof(AutoplayMode.ApplyEndStateOnStart) + ": same, but sets the last second state."
      )
    ] AutoplayMode _autoplay = AutoplayMode.Enabled;
    [SerializeField, TabGroup(TAB_FIELDS)] TweenTime _time = TweenTime.OnUpdate;
    [SerializeField, TabGroup(TAB_FIELDS)] TweenManager.Loop _looping = new TweenManager.Loop(1, TweenManager.Loop.Mode.Normal);
    [SerializeField, NotNull, TabGroup(TAB_FIELDS)] SerializedTweenTimeline _timeline;
    [
      SerializeField, NotNull, TLPCreateDerived, TabGroup(TAB_FIELDS)
    ] SerializedTweenCallback[] _onStart, _onEnd;
    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 649
    #endregion

    TweenManager _manager;

    [PublicAPI]
    public TweenManager manager {
      get {
        TweenManager create() {
          if (Log.d.isDebug()) Log.d.debug($"Creating {nameof(TweenManager)} for {this}", this);
          var tm = new TweenManager(_timeline.timeline, _time, _looping, context: gameObject);
          foreach (var cb in _onStart) tm.addOnStartCallback(cb.callback.callback);
          foreach (var cb in _onEnd) tm.addOnEndCallback(cb.callback.callback);
          return tm;
        }

        return _manager ?? (_manager = create());
      }
    }

    bool lastStateWasPlaying;

    public void Start() {
      // Create manager on start.
      manager.forSideEffects();
      handleStartAutoplay();
    }

    void handleStartAutoplay() {
      if (_autoplay == AutoplayMode.ApplyZeroStateOnStart || _autoplay == AutoplayMode.EnabledAndApplyZeroStateOnStart)
        applyZeroState();
      else if (_autoplay == AutoplayMode.ApplyEndStateOnStart) applyMaxDurationState();
    }

    public void OnEnable() {
      if (_autoplay == AutoplayMode.Enabled || _autoplay == AutoplayMode.EnabledAndApplyZeroStateOnStart) playForwards();
      else if (_runMode == RunMode.Local && lastStateWasPlaying) resume();
    }

    public void OnDisable() {
      if (_runMode == RunMode.Local && lastStateWasPlaying) manager.stop();
    }

    public void OnDestroy() {
      if (_runMode == RunMode.Local) manager.stop();
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS)]
    void playForwards() {
      manager.play(forwards: true);
      lastStateWasPlaying = true;
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS)]
    void playBackwards() {
      manager.play(forwards: false);
      lastStateWasPlaying = true;
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS)]
    void resumeForwards() {
      manager.resume(forwards: true); 
      lastStateWasPlaying = true;
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS)]
    void resumeBackwards() {
      manager.resume(forwards: false);
      lastStateWasPlaying = true;
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS)]
    void resume() {
      manager.resume();
      lastStateWasPlaying = true;
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS)]
    void stop() {
      manager.stop();
      lastStateWasPlaying = false;
    }

    [ShowInInspector, TabGroup(TAB_ACTIONS)] void reverse() => manager.reverse();
    [ShowInInspector, TabGroup(TAB_ACTIONS)] void rewind() => 
      manager.rewind(applyEffectsForRelativeTweens: false);
    [ShowInInspector, TabGroup(TAB_ACTIONS)] void rewindWithEffectsForRelative() => 
      manager.rewind(applyEffectsForRelativeTweens: true);
    
    [ShowInInspector, TabGroup(TAB_ACTIONS)] void applyZeroState() =>
      manager.timeline.applyStateAt(0);
    [ShowInInspector, TabGroup(TAB_ACTIONS)] void applyMaxDurationState() =>
      manager.timeline.applyStateAt(manager.timeline.duration);

#if UNITY_EDITOR
    [ShowInInspector, UsedImplicitly, TabGroup(TAB_FIELDS)]
    // Advanced Inspector does not render a button if it implements interface method. 
    void recreate() => invalidate();
#endif

    public void invalidate() {
      if (_manager != null) {
        _manager.stop();
      }
      lastStateWasPlaying = false;
      _timeline.invalidate();
      _manager = null;
      handleStartAutoplay();
      if (_autoplay == AutoplayMode.Enabled || _autoplay == AutoplayMode.EnabledAndApplyZeroStateOnStart) manager.play();
    }
    
    public enum Action {
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
    [PublicAPI] public void run(Action action) {
      switch (action) {
        case Action.PlayForwards:
          playForwards();
          break;
        case Action.PlayBackwards:
          playBackwards();
          break;
        case Action.ResumeForwards:
          resumeForwards();
          break;
        case Action.ResumeBackwards:
          resumeBackwards();
          break;
        case Action.Resume:
          resume();
          break;
        case Action.Stop:
          stop();
          break;
        case Action.Reverse:
          reverse();
          break;
        case Action.Rewind:
          rewind();
          break;
        case Action.RewindWithEffectsForRelative:
          rewindWithEffectsForRelative();
          break;
        case Action.ApplyZeroState:
          applyZeroState();
          break;
        case Action.ApplyMaxDurationState:
          applyMaxDurationState();
          break;
        default: throw new Exception($"Unknown action {action} for {nameof(FunTweenManager)}");
      }
    }
  }
}