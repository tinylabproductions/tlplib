using System;
using System.Collections.Generic;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.validations;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  public abstract class SerializedTweener : SerializedTweenTimelineElement {}

  public abstract class SerializedTweener<SourceType, DestinationType, Target> : SerializedTweener {
    [SerializeField] bool _isRelative = true;
    [SerializeField, NotNull] SourceType _start, _end;
    [SerializeField, Tooltip("in seconds")] float _duration = 1;
    [SerializeField, NotNull] SerializedEase _ease;
    [SerializeField, NotNull, NonEmpty] Target[] _targets = new Target[1];

    readonly TweenMutator<DestinationType, Target> mutator;
    readonly Tween<DestinationType>.Ops ops;
    readonly Fn<SourceType, DestinationType> convert;

    protected SerializedTweener(
      Tween<DestinationType>.Ops ops, TweenMutator<DestinationType, Target> mutator,
      Fn<SourceType, DestinationType> convert, SourceType defaultValue
    ) {
      this.ops = ops;
      this.mutator = mutator;
      this.convert = convert;
      _start = _end = defaultValue;
    }

    public override float duration => _duration;
    public override IEnumerable<TweenTimelineElement> elements {
      get {
        var tween = new Tween<DestinationType>(
          convert(_start), convert(_end), _isRelative, _ease.ease, ops, _duration
        );
        return _targets.map(target => new Tweener<DestinationType, Target>(tween, target, mutator));
      }
    }

    public override string ToString() {
      var changeS =
        _isRelative
          ? ops.diff(convert(_end), convert(_start)).ToString()
          : $"{_start} to {_end}";
      return $"{changeS} over {_duration}s with {_ease} on {_targets.Length} targets";
    }
  }

  public abstract class SerializedTweener<Value, Target> : SerializedTweener<Value, Value, Target> {
    protected SerializedTweener(
      Tween<Value>.Ops ops, TweenMutator<Value, Target> mutator, Value defaultValue
    ) : base(ops, mutator, _ => _, defaultValue) {}
  }
}