using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [Serializable]
  public abstract class SerializedTweenerV2<TObject, TValue>
    : ISerializedTweenTimelineElement, TweenTimelineElement, IApplyStateAt
  where TValue : struct
  {
    const string START = "start";
    const string END = "end";
    const string DELTA = "delta";
    const string DURATION = "duration";
    const int LABEL_WIDTH = 50;
    const string CHANGE = "editorSetDirty";
    const string SHOW_DELTA = nameof(displayAsDelta);

    [SerializeField, OnValueChanged(CHANGE), PropertyOrder(-1), NotNull] protected TObject _target;
    [SerializeField, OnValueChanged(CHANGE), HideLabel, HorizontalGroup(START)] protected TValue _start;
    [SerializeField, OnValueChanged(CHANGE), HideLabel, HorizontalGroup(END), HideIf(SHOW_DELTA)] protected TValue _end;
    [SerializeField, OnValueChanged(CHANGE), HorizontalGroup(DURATION, Width = 90), LabelWidth(55)] float _duration = 1;
    [SerializeField, OnValueChanged(CHANGE), HorizontalGroup(DURATION, MarginLeft = 20, Width = 210), HideLabel] 
    SerializedEaseV2 _ease;

    public TweenTimelineElement toTimelineElement() {
#if UNITY_EDITOR
      __editorDirty = false;
#endif
      return this;
    }

    public Object getTarget() => _target as Object;

    public float duration => _duration;

    protected abstract TValue lerp(float percentage);
    protected abstract TValue add(TValue a, TValue b);
    protected abstract TValue subtract(TValue a, TValue b);
    protected abstract TValue get { get; }
    protected abstract void set(TValue value);

    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) => applyStateAt(timePassed);

    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = this;
      return true;
    }

    public void applyStateAt(float time) => set(lerp(_ease.ease.Invoke(time / duration)));

    [ShowInInspector, PropertyOrder(-1), LabelText("Current"), LabelWidth(LABEL_WIDTH), ShowIf(nameof(showCurrent))] 
    TValue __current {
      get {
        try { return get; } catch (Exception _) { return default; }
      }
    }
    
    bool showCurrent => SerializedTweenTimelineV2.editorDisplayCurrent && hasTarget;

    // Equals(null) checks if unity object is alive
    bool hasTarget => _target != null && !_target.Equals(null);

    bool displayAsDelta => SerializedTweenTimelineV2.editorDisplayEndAsDelta;
    
    protected static string[] spQuaternion(string sp) => new[] { $"{sp}.x", $"{sp}.y", $"{sp}.z", $"{sp}.w" };
    protected static string[] spVector3(string sp) => new[] { $"{sp}.x", $"{sp}.y", $"{sp}.z" };
    protected static string[] spVector2(string sp) => new[] { $"{sp}.x", $"{sp}.y" };
    
#if UNITY_EDITOR
    
    public void trySetDuration(float duration) => _duration = duration;

    public bool __editorDirty { get; private set; } = true;
    public abstract string[] __editorSerializedProps { get; }
    [UsedImplicitly] void editorSetDirty() => __editorDirty = true;
    
    [Button("Start"), PropertyOrder(-1), HorizontalGroup(START, Width = LABEL_WIDTH)]
    void __setStart() => _start = get;
    [Button("End"), PropertyOrder(-1), HorizontalGroup(END, Width = LABEL_WIDTH), HideIf(SHOW_DELTA)] 
    void __setEnd() => _end = get;
    [Button("Delta"), PropertyOrder(-1), HorizontalGroup(DELTA, Width = LABEL_WIDTH), ShowIf(SHOW_DELTA)] 
    void __setDelta() => _delta = get;

    [OnValueChanged(CHANGE), HideLabel, HorizontalGroup(DELTA), ShowIf(SHOW_DELTA), ShowInInspector]
    TValue _delta {
      get => subtract(_end, _start);
      set => _end = add(_start, value);
    }
#endif
  }

  public abstract class SerializedTweenerVector3<T> : SerializedTweenerV2<T, Vector3> {
    protected override Vector3 lerp(float percentage) => Vector3.Lerp(_start, _end, percentage);
    protected override Vector3 add(Vector3 a, Vector3 b) => a + b;
    protected override Vector3 subtract(Vector3 a, Vector3 b) => a - b;
  }

  public abstract class SerializedTweenerVector2<T> : SerializedTweenerV2<T, Vector2> {
    protected override Vector2 lerp(float percentage) => Vector2.Lerp(_start, _end, percentage);
    protected override Vector2 add(Vector2 a, Vector2 b) => a + b;
    protected override Vector2 subtract(Vector2 a, Vector2 b) => a - b;
  }
  
  public abstract class SerializedTweenerFloat<T> : SerializedTweenerV2<T, float> {
    protected override float lerp(float percentage) => Mathf.Lerp(_start, _end, percentage);
    protected override float add(float a, float b) => a + b;
    protected override float subtract(float a, float b) => a - b;
  }

  // ReSharper disable NotNullMemberIsNotInitialized

  [Serializable]
  public sealed class AnchoredPosition : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => _target.anchoredPosition;
    protected override void set(Vector2 value) => _target.anchoredPosition = value;

    public override string[] __editorSerializedProps => spVector2("m_AnchoredPosition");
  }

  [Serializable]
  public sealed class LocalScale : SerializedTweenerVector3<Transform> {
    protected override Vector3 get => _target.localScale;
    protected override void set(Vector3 value) => _target.localScale = value;
    
    public override string[] __editorSerializedProps => spVector3("m_LocalScale");
  }
  
  [Serializable]
  public sealed class LocalPosition : SerializedTweenerVector3<Transform> {
    protected override Vector3 get => _target.localPosition;
    protected override void set(Vector3 value) => _target.localPosition = value;
    
    public override string[] __editorSerializedProps => spVector3("m_LocalPosition");
  }
  
  [Serializable]
  public sealed class Rotation2D : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localEulerAngles.z;
    protected override void set(float value) => _target.localEulerAngles = _target.localEulerAngles.withZ(value);
    
    public override string[] __editorSerializedProps => spQuaternion("m_LocalRotation");
  }

  // ReSharper restore NotNullMemberIsNotInitialized
}