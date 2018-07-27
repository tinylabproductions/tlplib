using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager {
  /// <summary>
  /// Serialized <see cref="TweenManager"/>.
  /// </summary>
  [AdvancedInspector(true)]
  public partial class FunTweenManager : MonoBehaviour, IMB_Start, IMB_OnEnable, IMB_OnDisable, IMB_OnDestroy, Invalidatable, IDataChanged {
    enum Tab { Fields, Actions }
    // ReSharper disable once UnusedMember.Local
    enum RunMode : byte { Local, Global }
    // ReSharper disable once UnusedMember.Local
    enum AutoplayMode : byte {
      Disabled = 0, Enabled = 1, ApplyZeroStateOnStart = 2, ApplyEndStateOnStart = 3 
    }

    [Inspect, Tab(Tab.Actions), UsedImplicitly, ReadOnly]
    float timePassed => _manager == null ? -1 : _manager.timeline.timePassed;
    [Inspect, Tab(Tab.Actions), UsedImplicitly, ReadOnly]
    uint currentIteration => _manager == null ? 0 : _manager.currentIteration;
    [Inspect, Tab(Tab.Actions), UsedImplicitly, ReadOnly]
    float timescale => _manager == null ? -1 : _manager.timescale;
    
    [
      SerializeField, Tab(Tab.Fields),
      Help(
        HelpType.Info, 
        "Local mode pauses tweens when this game object is disabled and resumes when it is enabled.\n" +
        "Global mode continues to run the tween even if irrespective of this game objects state."
      )
    ] RunMode _runMode = RunMode.Local;
    [
      SerializeField, Tab(Tab.Fields),
      Help(
        HelpType.Info,
        "Modes:\n" +
        "- " + nameof(AutoplayMode.Disabled) + ": does nothing.\n" +
        "- " + nameof(AutoplayMode.Enabled) + ": starts playing forwards upon enabling this game object. Pauses upon " +
        "disabling game object, resumes when it is enabled again.\n" +
        "- " + nameof(AutoplayMode.ApplyZeroStateOnStart) + ": when this script receives Unity Start callback, " +
        "sets all the properties of non-relatively tweened objects like it was zeroth second in the timeline.\n" +
        "- " + nameof(AutoplayMode.ApplyEndStateOnStart) + ": same, but sets the last second state."
      )
    ] AutoplayMode _autoplay = AutoplayMode.Enabled;
    [SerializeField, Tab(Tab.Fields)] TweenTime _time = TweenTime.OnUpdate;
    [SerializeField, Tab(Tab.Fields)] TweenManager.Loop _looping = new TweenManager.Loop(1, TweenManager.Loop.Mode.Normal);
    [SerializeField, NotNull, Tab(Tab.Fields), PublicAccessor] SerializedTweenTimeline _timeline;
    [SerializeField, NotNull, CreateDerived, Tab(Tab.Fields)] SerializedTweenCallback[] _onStart, _onEnd;

    TweenManager _manager;

    [PublicAPI]
    public TweenManager manager {
      get {
        TweenManager create() {
          if (Log.d.isDebug()) Log.d.debug($"Creating {nameof(TweenManager)} for {this}", this);
          var tm = new TweenManager(_timeline.timeline, _time, _looping);
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
      if (_autoplay == AutoplayMode.ApplyZeroStateOnStart) applyZeroState();
      else if (_autoplay == AutoplayMode.ApplyEndStateOnStart) applyMaxDurationState();
    }

    public void OnEnable() {
      if (_autoplay == AutoplayMode.Enabled) playForwards();
      else if (_runMode == RunMode.Local && lastStateWasPlaying) resume();
    }

    public void OnDisable() {
      if (_runMode == RunMode.Local && lastStateWasPlaying) manager.stop();
    }

    public void OnDestroy() {
      if (_runMode == RunMode.Local) manager.stop();
    }

    [Inspect, Tab(Tab.Actions)]
    void playForwards() {
      manager.play(forwards: true);
      lastStateWasPlaying = true;
    }

    [Inspect, Tab(Tab.Actions)]
    void playBackwards() {
      manager.play(forwards: false);
      lastStateWasPlaying = true;
    }

    [Inspect, Tab(Tab.Actions)]
    void resumeForwards() {
      manager.resume(forwards: true); 
      lastStateWasPlaying = true;
    }

    [Inspect, Tab(Tab.Actions)]
    void resumeBackwards() {
      manager.resume(forwards: false);
      lastStateWasPlaying = true;
    }

    [Inspect, Tab(Tab.Actions)]
    void resume() {
      manager.resume();
      lastStateWasPlaying = true;
    }

    [Inspect, Tab(Tab.Actions)]
    void stop() {
      manager.stop();
      lastStateWasPlaying = false;
    }

    [Inspect, Tab(Tab.Actions)] void reverse() => manager.reverse();
    [Inspect, Tab(Tab.Actions)] void rewind() => 
      manager.rewind(applyEffectsForRelativeTweens: false);
    [Inspect, Tab(Tab.Actions)] void rewindWithEffectsForRelative() => 
      manager.rewind(applyEffectsForRelativeTweens: true);
    
    [Inspect, Tab(Tab.Actions)] void applyZeroState() =>
      manager.timeline.applyStateAt(0);
    [Inspect, Tab(Tab.Actions)] void applyMaxDurationState() =>
      manager.timeline.applyStateAt(manager.timeline.duration);

#if UNITY_EDITOR
    [Inspect, UsedImplicitly, Tab(Tab.Fields)]
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
      if (_autoplay == AutoplayMode.Enabled) manager.play();
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
        default:
          Log.d.error($"Unknown action {action} for {nameof(FunTweenManager)}");
          break;
      }
    }

    public void DataChanged() { Log.d.warn("Fun tween manager has been changed"); }
    public event GenericEventHandler OnDataChanged;
  }
}