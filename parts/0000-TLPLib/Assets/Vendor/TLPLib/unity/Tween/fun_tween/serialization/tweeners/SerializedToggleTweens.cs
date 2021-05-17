using System;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using JetBrains.Annotations;
using pzd.lib.exts;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  
  /// <summary>
  /// A tween that enables something while it is playing and disables when tween ends.
  /// </summary>
  [Serializable]
  public abstract class ToggleTweenBase<TObject> : ISerializedTweenTimelineElementBase, TweenTimelineElement {
    protected const string CHANGE = "editorSetDirty";
    
#pragma warning disable 649
    [SerializeField, OnValueChanged(CHANGE)] float _duration = 1;
    [SerializeField, OnValueChanged(CHANGE), PropertyOrder(-1), NotNull] protected TObject _target;
    // [SerializeField, OnValueChanged(CHANGE)] InvokeOn _invokeOn;
#pragma warning restore 649
    
    public TweenTimelineElement toTimelineElement() {
#if UNITY_EDITOR
      __editorDirty = false;
#endif
      return this;
    }
    
    public float duration => _duration;
    public void trySetDuration(float val) => _duration = val;
    
    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens, 
      bool exitTween
    ) {
      var prevInRange = timeInRange(previousTimePassed);
      var nextInRange = !exitTween;
      
      if (prevInRange != nextInRange || exitTween) invoke(nextInRange);
    }

    bool timeInRange(float time) {
      return time > 0 && time < _duration;
    }
    
    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = null;
      return false;
    }

    protected abstract void invoke(bool value);
    
    public TObject target => _target;
    public Object getTarget() => _target as Object;
    
    // Equals(null) checks if unity object is alive
    protected bool hasTarget => _target != null && !_target.Equals(null);
    public bool isValid => hasTarget;
    public Color editorColor => Color.white;

#if UNITY_EDITOR
    public bool __editorDirty { get; private set; } = true;
    [UsedImplicitly] void editorSetDirty() => __editorDirty = true;
#endif
  }
  
  [Serializable]
  public class ToggleGameObject : ToggleTweenBase<GameObject> {
    protected override void invoke(bool value) => _target.SetActive(value);
  }
  
  [Serializable]
  public class ToggleMonoBehaviour : ToggleTweenBase<MonoBehaviour> {
    protected override void invoke(bool value) => _target.enabled = value;
  }
  
  [Serializable]
  public class ToggleRenderer : ToggleTweenBase<Renderer> {
    protected override void invoke(bool value) => _target.enabled = value;
  }
}