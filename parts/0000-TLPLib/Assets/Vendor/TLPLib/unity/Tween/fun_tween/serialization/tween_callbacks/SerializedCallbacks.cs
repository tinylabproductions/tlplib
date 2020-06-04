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
    
    [SerializeField, OnValueChanged(CHANGE)] InvokeOn _invokeOn;
    
    public TweenTimelineElement toTimelineElement() => this;
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
    [InfoBox("Use false for better performance")]
    [SerializeField, OnValueChanged(CHANGE)] bool _withChildren;

    protected override void invoke(ParticleSystem ps) {
      ps.Play(withChildren: _withChildren);
    }
  }
  
  [Serializable]
  public sealed class ParticleSystemStop : ParticleSystemBase {
    [InfoBox("Use false for better performance")]
    [SerializeField, OnValueChanged(CHANGE)] bool _withChildren;
    [SerializeField, OnValueChanged(CHANGE)] ParticleSystemStopBehavior _stopBehavior = ParticleSystemStopBehavior.StopEmitting;

    protected override void invoke(ParticleSystem ps) {
      ps.Stop(withChildren: _withChildren, stopBehavior: _stopBehavior);
    }
  }
  
  [Serializable]
  public class TweenManagerCallback : CallbackBase {
    [SerializeField, NotNull, OnValueChanged(CHANGE)] FunTweenManagerV2 _manager;
    [SerializeField, OnValueChanged(CHANGE)] FunTweenManagerV2.Action _action = FunTweenManagerV2.Action.PlayForwards;

    protected override void invoke() => _manager.run(_action);

    public override bool isValid => _manager;

    public override Object getTarget() => _manager;
  }
  
  [Serializable]
  public class EnableGameObjectCallback : CallbackBase {
    [SerializeField, NotNull, OnValueChanged(CHANGE)] GameObject _gameObject;
    [SerializeField, OnValueChanged(CHANGE)] bool _state;

    protected override void invoke() => _gameObject.SetActive(_state);

    public override bool isValid => _gameObject;

    public override Object getTarget() => _gameObject;
  }
  
  // ReSharper restore NotNullMemberIsNotInitialized
}