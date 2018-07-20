using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.validations;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  public abstract class SerializedTweener : SerializedTweenTimelineElement {
    // ReSharper disable once UnusedMember.Global
    protected enum Mode { Absolute = 0, Relative = 1, RelativeFromCreation = 2 }
  }

  public abstract class SerializedTweener<SourceType, DestinationType, Target> : SerializedTweener {
    
#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
    [SerializeField, FormerlySerializedAs("_isRelative")] Mode _mode = Mode.RelativeFromCreation;
    [SerializeField, NotNull] SourceType _start, _end;
    [SerializeField, Tooltip("in seconds")] float _duration = 1;
    [SerializeField, NotNull] SerializedEase _ease;
    [SerializeField, NotNull, NonEmpty] Target[] _targets = new Target[1];
    // ReSharper restore FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
#pragma warning restore 649

    readonly Tween<DestinationType>.Ops ops;
    readonly SerializedTweenerOps.Add<DestinationType> add;
    readonly SerializedTweenerOps.Extract<DestinationType, Target> extract;
    protected abstract TweenMutator<DestinationType, Target> mutator { get; }
    protected abstract DestinationType convert(SourceType value);

    protected SerializedTweener(
      Tween<DestinationType>.Ops ops, 
      SerializedTweenerOps.Add<DestinationType> add, 
      SerializedTweenerOps.Extract<DestinationType, Target> extract, 
      SourceType defaultValue
    ) {
      this.ops = ops;
      this.add = add;
      this.extract = extract;
      _start = _end = defaultValue;
    }

    public override void invalidate() => _ease.invalidate();

    public override float duration => _duration;
    public override IEnumerable<TweenTimelineElement> elements {
      get {
        Tween<DestinationType> _tween = null;
        Tween<DestinationType> getTween(DestinationType current) {
          if (_mode == Mode.RelativeFromCreation) {
            var start = add(current, convert(_start));
            var end = add(current, convert(_end));
            return new Tween<DestinationType>(start, end, false, _ease.ease, ops, _duration);
          }
          else {
            return _tween ?? (_tween = new Tween<DestinationType>(
              convert(_start), convert(_end), _mode == Mode.Relative, _ease.ease, ops, _duration
            ));
          }
        }
        
        return _targets.map(target => 
          new Tweener<DestinationType, Target>(getTween(extract(target)), target, mutator)
        );
      }
    }

    public override string ToString() {
      var changeS =
        _mode == Mode.Relative ? ops.diff(convert(_end), convert(_start)).ToString()
        : _mode == Mode.RelativeFromCreation ? $"current + ({_start} to {_end})"
        : $"{_start} to {_end}";
      return $"{changeS} over {_duration}s with {_ease} on {_targets.Length} targets";
    }
  }

  public abstract class SerializedTweener<Value, Target> : SerializedTweener<Value, Value, Target> {
    protected override TweenMutator<Value, Target> mutator { get; }
    protected override Value convert(Value value) => value;

    protected SerializedTweener(
      Tween<Value>.Ops ops, SerializedTweenerOps.Add<Value> add, SerializedTweenerOps.Extract<Value, Target> extract,
      TweenMutator<Value, Target> mutator, Value defaultValue
    ) : base(ops, add, extract, defaultValue) {
      this.mutator = mutator;
    }
  }
}