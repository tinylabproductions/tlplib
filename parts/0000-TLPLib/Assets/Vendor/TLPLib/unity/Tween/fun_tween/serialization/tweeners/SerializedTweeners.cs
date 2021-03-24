using System;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [Serializable]
  public abstract class SerializedTweenerV2Base<TObject>
    : ISerializedTweenTimelineElement, TweenTimelineElement, IApplyStateAt
  {
    // Don't use nameof, because those fields exist only in UNITY_EDITOR
    protected const string CHANGE = "editorSetDirty";

    [SerializeField, OnValueChanged(CHANGE), PropertyOrder(-1), NotNull] protected TObject _target;

    public TweenTimelineElement toTimelineElement() {
#if UNITY_EDITOR
      __editorDirty = false;
#endif
      return this;
    }

    public Object getTarget() => _target as Object;

    public abstract float duration { get; }

    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) => applyStateAt(timePassed);

    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = this;
      return true;
    }

    // Equals(null) checks if unity object is alive
    protected bool hasTarget => _target != null && !_target.Equals(null);

    public abstract void trySetDuration(float duration);
    public bool isValid => hasTarget;
    
#if UNITY_EDITOR
    public bool __editorDirty { get; protected set; } = true;
    [UsedImplicitly] void editorSetDirty() => __editorDirty = true;
#endif
    public abstract void applyStateAt(float time);
  }
  
  
  
  [Serializable]
  public abstract class SerializedTweenerV2<TObject, TValue> : SerializedTweenerV2WithTarget<TObject, TValue, TValue>
    where TValue : struct
  {

    #if UNITY_EDITOR
    protected override void __setStart() => _start = get;
    protected override void __setEnd() => _end = get;
    protected override void __setDelta() => _delta = get;

    [OnValueChanged(CHANGE), HideLabel, HorizontalGroup(DELTA), ShowIf(SHOW_DELTA), ShowInInspector]
    TValue _delta {
      get => subtract(_end, _start);
      set => _end = add(_start, value);
    }
    #endif
  }

  [Serializable]
  public abstract class SerializedTweenerV2WithTarget<TObject, TValue, TTarget> : SerializedTweenerV2Base<TObject>
    where TValue : struct
  {
    const string START = "start";
    const string END = "end";
    protected const string DELTA = "delta";
    const string DURATION = "duration";
    const int LABEL_WIDTH = 50;
    
    // Don't use nameof, because those fields exist only in UNITY_EDITOR
    const string SHOW_CURRENT = "showCurrent";
    protected const string SHOW_DELTA = "displayAsDelta";

    [SerializeField, OnValueChanged(CHANGE), HideLabel, HorizontalGroup(START)] protected TTarget _start;
    [SerializeField, OnValueChanged(CHANGE), HideLabel, HorizontalGroup(END), HideIf(SHOW_DELTA)] protected TTarget _end;
    [SerializeField, OnValueChanged(CHANGE), HorizontalGroup(DURATION, Width = 90), LabelWidth(55)] float _duration = 1;
    [
      SerializeField, OnValueChanged(CHANGE), HorizontalGroup(DURATION, MarginLeft = 20, Width = 210), HideLabel
    ] SerializedEaseV2 _ease;

    public override float duration => _duration;

    protected abstract TValue lerp(float percentage);
    protected abstract TValue add(TValue a, TValue b);
    protected abstract TValue subtract(TValue a, TValue b);
    protected abstract TValue get { get; }
    protected abstract void set(TValue value);

    // TODO: cache ease function
    public override void applyStateAt(float time) => set(lerp(_ease.ease.Invoke(time / duration)));

    [ShowInInspector, PropertyOrder(-1), LabelText("Current"), LabelWidth(LABEL_WIDTH), ShowIf(SHOW_CURRENT)] 
    TValue __current {
      get {
        try { return get; } catch (Exception) { return default; }
      }
    }

    public override void trySetDuration(float duration) => _duration = duration;

    // protected static string[] spQuaternion(string sp) => new[] { $"{sp}.x", $"{sp}.y", $"{sp}.z", $"{sp}.w" };
    // protected static string[] spVector3(string sp) => new[] { $"{sp}.x", $"{sp}.y", $"{sp}.z" };
    // protected static string[] spVector2(string sp) => new[] { $"{sp}.x", $"{sp}.y" };
    
#if UNITY_EDITOR
    [UsedImplicitly] bool showCurrent => SerializedTweenTimelineV2.editorDisplayCurrent && hasTarget;
    [UsedImplicitly] bool displayAsDelta => SerializedTweenTimelineV2.editorDisplayEndAsDelta;

    [Button("Start"), PropertyOrder(-1), HorizontalGroup(START, Width = LABEL_WIDTH)]
    protected virtual void __setStart() { }
    [Button("End"), PropertyOrder(-1), HorizontalGroup(END, Width = LABEL_WIDTH), HideIf(SHOW_DELTA)]
    protected virtual void __setEnd() { }
    [Button("Delta"), PropertyOrder(-1), HorizontalGroup(DELTA, Width = LABEL_WIDTH), ShowIf(SHOW_DELTA)]
    protected virtual void __setDelta() { }

    [Button] void swapEndAndStart() {
      var copy = _start;
      _start = _end;
      _end = copy;
    }
#endif
  }

  
  
  public abstract class SerializedTweenerVector3<T> : SerializedTweenerV2<T, Vector3> {
    protected override Vector3 lerp(float percentage) => Vector3.LerpUnclamped(_start, _end, percentage);
    protected override Vector3 add(Vector3 a, Vector3 b) => a + b;
    protected override Vector3 subtract(Vector3 a, Vector3 b) => a - b;
  }

  public abstract class SerializedTweenerVector2<T> : SerializedTweenerV2<T, Vector2> {
    protected override Vector2 lerp(float percentage) => Vector2.LerpUnclamped(_start, _end, percentage);
    protected override Vector2 add(Vector2 a, Vector2 b) => a + b;
    protected override Vector2 subtract(Vector2 a, Vector2 b) => a - b;
  }
  
  public abstract class SerializedTweenerFloat<T> : SerializedTweenerV2<T, float> {
    protected override float lerp(float percentage) => Mathf.LerpUnclamped(_start, _end, percentage);
    protected override float add(float a, float b) => a + b;
    protected override float subtract(float a, float b) => a - b;
  }
  
  public abstract class SerializedTweenerInt<T> : SerializedTweenerV2<T, int> {
    protected override int lerp(float percentage) => (int) Mathf.LerpUnclamped(_start, _end, percentage);
    protected override int add(int a, int b) => a + b;
    protected override int subtract(int a, int b) => a - b;
  }
  
  public abstract class SerializedTweenerColor<T> : SerializedTweenerV2<T, Color> {
    protected override Color lerp(float percentage) => Color.LerpUnclamped(_start, _end, percentage);
    protected override Color add(Color a, Color b) => a + b;
    protected override Color subtract(Color a, Color b) => a - b;
  }
  
  [Serializable]
  public abstract class SerializedTweenerUnit<TObject> : SerializedTweenerV2Base<TObject> {
    [SerializeField] float _duration = 1;

    public override float duration => _duration;

    public override void trySetDuration(float d) => _duration = d;
  }

  // ReSharper disable NotNullMemberIsNotInitialized

  [Serializable]
  public sealed class PositionBetweenTargets : SerializedTweenerV2WithTarget<Transform, Vector2, Transform> {
    protected override Vector2 lerp(float percentage) => Vector2.LerpUnclamped(_start.position, _end.position, percentage);
    protected override Vector2 add(Vector2 a, Vector2 b) => a + b;
    protected override Vector2 subtract(Vector2 a, Vector2 b) => a - b;
    protected override Vector2 get => _target.position;
    protected override void set(Vector2 value) => _target.position = value;
  }

  [Serializable]
  public sealed class AnchoredPosition : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => _target.anchoredPosition;
    protected override void set(Vector2 value) => _target.anchoredPosition = value;

    // public override string[] __editorSerializedProps => spVector2("m_AnchoredPosition");
  }
  
  [Serializable]
  public sealed class AnchoredPositionX : SerializedTweenerFloat<RectTransform> {
    protected override float get => _target.anchoredPosition.x;
    protected override void set(float value) {
      var pos = _target.anchoredPosition;
      pos.x = value;
      _target.anchoredPosition = pos;
    }
  }
  
  [Serializable]
  public sealed class AnchoredPositionY : SerializedTweenerFloat<RectTransform> {
    protected override float get => _target.anchoredPosition.y;
    protected override void set(float value) {
      var pos = _target.anchoredPosition;
      pos.y = value;
      _target.anchoredPosition = pos;
    }
  }
  
  [Serializable]
  public sealed class RectTransformOffsetMin : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => _target.offsetMin;
    protected override void set(Vector2 value) => _target.offsetMin = value;
  }
  
  [Serializable]
  public sealed class RectTransformOffsetMax : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => _target.offsetMax;
    protected override void set(Vector2 value) => _target.offsetMax = value;
  }

  [Serializable]
  public sealed class LocalScale : SerializedTweenerVector3<Transform> {
    protected override Vector3 get => _target.localScale;
    protected override void set(Vector3 value) => _target.localScale = value;
    
    // public override string[] __editorSerializedProps => spVector3("m_LocalScale");
  }

  [Serializable] public sealed class LocalScaleX : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localScale.x;
    protected override void set(float value) => _target.localScale = _target.localScale.withX(value);
  }
  [Serializable] public sealed class LocalScaleY : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localScale.y;
    protected override void set(float value) => _target.localScale = _target.localScale.withY(value);
  }
  [Serializable] public sealed class LocalScaleZ : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localScale.z;
    protected override void set(float value) => _target.localScale = _target.localScale.withZ(value);
  }
  
  
  [Serializable]
  public sealed class LocalPosition : SerializedTweenerVector3<Transform> {
    protected override Vector3 get => _target.localPosition;
    protected override void set(Vector3 value) => _target.localPosition = value;
    
    // public override string[] __editorSerializedProps => spVector3("m_LocalPosition");
  }
  
  [Serializable]
  public sealed class LocalPositionX : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localPosition.x;
    protected override void set(float value) {
      var pos = _target.localPosition;
      pos.x = value;
      _target.localPosition = pos;
    }
  }
  
  [Serializable]
  public sealed class LocalPositionY : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localPosition.y;
    protected override void set(float value) {
      var pos = _target.localPosition;
      pos.y = value;
      _target.localPosition = pos;
    }
  }
  
  [Serializable]
  public sealed class LocalPositionZ : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localPosition.z;
    protected override void set(float value) {
      var pos = _target.localPosition;
      pos.z = value;
      _target.localPosition = pos;
    }
  }
  
  [Serializable]
  public sealed class LocalRotation2D : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localEulerAngles.z;
    protected override void set(float value) => _target.localEulerAngles = _target.localEulerAngles.withZ(value);
    
    // public override string[] __editorSerializedProps => spQuaternion("m_LocalRotation");
  }

  [Serializable]
  public sealed class LocalRotationX : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localEulerAngles.x;
    protected override void set(float value) => _target.localEulerAngles = _target.localEulerAngles.withX(value);
  }

  [Serializable]
  public sealed class LocalRotationY : SerializedTweenerFloat<Transform> {
    protected override float get => _target.localEulerAngles.y;
    protected override void set(float value) => _target.localEulerAngles = _target.localEulerAngles.withY(value);
  }
  
  [Serializable]
  public sealed class ImageColor : SerializedTweenerColor<Image> {
    protected override Color get => _target.color;
    protected override void set(Color value) => _target.color = value;
  }
  
  [Serializable]
  public sealed class ImageAlpha : SerializedTweenerFloat<Image> {
    protected override float get => _target.color.a;
    protected override void set(float value) => _target.color = _target.color.withAlpha(value);
  }
  
  [Serializable]
  public sealed class CustomImageColor : SerializedTweenerColor<CustomImage> {
    protected override Color get => _target.color;
    protected override void set(Color value) => _target.color = value;
  }
  
  [Serializable]
  public sealed class SpriteRendererColor : SerializedTweenerColor<SpriteRenderer> {
    protected override Color get => _target.color;
    protected override void set(Color value) => _target.color = value;
  }
  
  [Serializable]
  public sealed class TextMeshColor : SerializedTweenerColor<TextMeshProUGUI> {
    protected override Color get => _target.color;
    protected override void set(Color value) => _target.color = value;
  }
  
  [Serializable]
  public sealed class CanvasGroupAlpha : SerializedTweenerFloat<CanvasGroup> {
    protected override float get => _target.alpha;
    protected override void set(float value) => _target.alpha = value;
  }
  
  [Serializable]
  public sealed class RectTransformSize : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => _target.sizeDelta;
    protected override void set(Vector2 value) => _target.sizeDelta = value;
  }
  
  [Serializable]
  public sealed class RectTransformSizeX : SerializedTweenerFloat<RectTransform> {
    protected override float get => _target.sizeDelta.x;
    protected override void set(float value) => _target.sizeDelta = _target.sizeDelta.withX(value);
  }
  
  [Serializable]
  public sealed class RectTransformSizeY : SerializedTweenerFloat<RectTransform> {
    protected override float get => _target.sizeDelta.y;
    protected override void set(float value) => _target.sizeDelta = _target.sizeDelta.withY(value);
  }
  
  [Serializable]
  public sealed class UpdateLayout : SerializedTweenerUnit<RectTransform> {
    public override void applyStateAt(float time) => LayoutRebuilder.MarkLayoutForRebuild(_target);
  }
  
  [Serializable]
  public sealed class RectTransformSimpleAnchors : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => _target.anchorMin;
    protected override void set(Vector2 value) => _target.anchorMin = _target.anchorMax = value;
  }
  
  [Serializable]
  public sealed class RectTransformSimpleAnchorsX : SerializedTweenerFloat<RectTransform> {
    protected override float get => _target.anchorMin.x;
    protected override void set(float value) {
      _target.anchorMin = _target.anchorMin.withX(value);
      _target.anchorMax = _target.anchorMax.withX(value);
    }
  }
  
  [Serializable]
  public sealed class RectTransformAnchorsX : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => new Vector2(_target.anchorMin.x, _target.anchorMax.x);
    protected override void set(Vector2 value) {
      _target.anchorMin = _target.anchorMin.withX(value.x);
      _target.anchorMax = _target.anchorMax.withX(value.y);
    }
  }
  
  [Serializable]
  public sealed class RectTransformAnchorsY : SerializedTweenerVector2<RectTransform> {
    protected override Vector2 get => new Vector2(_target.anchorMin.y, _target.anchorMax.y);
    protected override void set(Vector2 value) {
      _target.anchorMin = _target.anchorMin.withY(value.x);
      _target.anchorMax = _target.anchorMax.withY(value.y);
    }
  }
  
  [Serializable]
  public sealed class RectTransformSimpleAnchorsY : SerializedTweenerFloat<RectTransform> {
    protected override float get => _target.anchorMin.y;
    protected override void set(float value) {
      _target.anchorMin = _target.anchorMin.withY(value);
      _target.anchorMax = _target.anchorMax.withY(value);
    }
  }
  
  [Serializable]
  public sealed class MaterialColor : SerializedTweenerColor<Material> {
    protected override Color get => _target.color;
    protected override void set(Color value) => _target.color = value;
  }
  
  [Serializable]
  public sealed class ImageFillAmount : SerializedTweenerFloat<Image> {
    protected override float get => _target.fillAmount;
    protected override void set(float value) => _target.fillAmount = value;
  }

  // ReSharper restore NotNullMemberIsNotInitialized
}