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
  public abstract class SerializedTweener : SerializedTweenSequenceElement {}

  public abstract class SerializedTweener<SourceType, DestinationType, Target> : SerializedTweener {
    [SerializeField, NotNull] SourceType _start, _end;
    [SerializeField, Tooltip("in seconds")] float _duration;
    [SerializeField, NotNull] SerializedEase _ease;
    [SerializeField, NotNull, NonEmpty] Target[] _targets = F.emptyArray<Target>();

    protected abstract Act<DestinationType, Target> mutator { get; }
    protected abstract TweenLerp<DestinationType> lerp { get; }
    protected abstract DestinationType convert(SourceType src);

    public override IEnumerable<TweenSequenceElement> elements {
      get {
        var tween = new Tween<DestinationType>(convert(_start), convert(_end), _ease.ease, lerp, _duration);
        return _targets.map(target => new Tweener<DestinationType, Target>(tween, target, mutator));
      }
    }

    public override string ToString() =>
      $"{_start} to {_end} over {_duration}s with {_ease} on {_targets.Length} targets";
  }

  public abstract class SerializedTweener<Value, Target> : SerializedTweener<Value, Value, Target> {
    protected override Value convert(Value src) => src;
  }
}