using System;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.validations;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks {
  [Serializable]
  public abstract class CallbackBase : ISerializedTweenTimelineCallback, TweenTimelineElement {
    protected const string CHANGE = "editorSetDirty";
    
    protected enum InvokeOn : byte { Forward = 0, Backward = 1, Both = 2 }
    
#pragma warning disable 649
    [SerializeField, OnValueChanged(CHANGE)] InvokeOn _invokeOn;
#pragma warning restore 649
    
    public TweenTimelineElement toTimelineElement() {
#if UNITY_EDITOR
      __editorDirty = false;
#endif
      return this;
    }
    public float duration => 0;
    public void trySetDuration(float _) { }
    
    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) {
      var shouldInvoke = _invokeOn switch {
        InvokeOn.Forward => playingForwards,
        InvokeOn.Backward => !playingForwards,
        InvokeOn.Both => true,
        _ => false
      };
      if (shouldInvoke) invoke();
    }
    
    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = null;
      return false;
    }

    protected abstract void invoke();
    public abstract Object getTarget();
    public abstract bool isValid { get; }
    public Color editorColor => Color.white;

#if UNITY_EDITOR
    public bool __editorDirty { get; private set; } = true;
    [UsedImplicitly] void editorSetDirty() => __editorDirty = true;
#endif
  }
  
  // ReSharper disable NotNullMemberIsNotInitialized
  
  [Serializable]
  public abstract class ParticleSystemBase : CallbackBase {
    [SerializeField, NotNull, NonEmpty, OnValueChanged(CHANGE), ListDrawerSettings(Expanded = true)] 
    ParticleSystem[] _particleSystems = { null };

    protected override void invoke() {
      foreach (var ps in _particleSystems) invoke(ps);
    }

    public override bool isValid {
      get {
        if (_particleSystems.Length == 0) return false;
        foreach (var ps in _particleSystems) {
          if (!ps) return false;
        }
        return true;
      }
    }

    // TODO: do something better with multiple targets
    public override Object getTarget() => _particleSystems[0];

    protected abstract void invoke(ParticleSystem ps);
  }
  
  [Serializable]
  public sealed class ParticleSystemPlay : ParticleSystemBase {
#pragma warning disable 649
    [InfoBox("Use false for better performance")]
    [SerializeField, OnValueChanged(CHANGE)] bool _withChildren;
#pragma warning restore 649

    protected override void invoke(ParticleSystem ps) {
      ps.Play(withChildren: _withChildren);
    }
  }
  
  [Serializable]
  public sealed class ParticleSystemStop : ParticleSystemBase {
#pragma warning disable 649
    [InfoBox("Use false for better performance")]
    [SerializeField, OnValueChanged(CHANGE)] bool _withChildren;
    [SerializeField, OnValueChanged(CHANGE)] ParticleSystemStopBehavior _stopBehavior = ParticleSystemStopBehavior.StopEmitting;
#pragma warning restore 649
    
    protected override void invoke(ParticleSystem ps) {
      ps.Stop(withChildren: _withChildren, stopBehavior: _stopBehavior);
    }
  }
  
  [Serializable]
  public class TweenManagerCallback : CallbackBase {
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized
    [SerializeField, NotNull, OnValueChanged(CHANGE)] FunTweenManagerV2 _manager;
    [SerializeField, OnValueChanged(CHANGE)] FunTweenManagerV2.Action _action = FunTweenManagerV2.Action.PlayForwards;
    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649
    
    protected override void invoke() {
      // Playing tweens is not supported in edit mode.
      if (Application.isPlaying) _manager.run(_action);
    }

    public override bool isValid => _manager;

    public override Object getTarget() => _manager;
  }
  
  [Serializable]
  public class EnableGameObjectCallback : CallbackBase {
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized
    [SerializeField, NotNull, OnValueChanged(CHANGE)] GameObject _gameObject;
    [SerializeField, OnValueChanged(CHANGE)] bool _state;
    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649

    protected override void invoke() => _gameObject.SetActive(_state);

    public override bool isValid => _gameObject;

    public override Object getTarget() => _gameObject;
  }
  
  // ReSharper restore NotNullMemberIsNotInitialized
}