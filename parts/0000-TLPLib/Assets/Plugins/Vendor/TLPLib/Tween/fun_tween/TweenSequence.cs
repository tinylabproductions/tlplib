using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class TweenMutators {
    public static readonly Act<Vector3, Transform> 
      position = (v, t) => t.position = v,
      localPosition = (v, t) => t.localPosition = v;
  }

  public enum TweenPlayback : byte { Forward, Backward }

  public interface TweenSequenceElement {
    float duration { get; }
    void setRelativeTimePassed(float t);
  }

  public class TweenSequence : TweenSequenceElement {
    struct Effect {
      public readonly float startsAt, endsAt;
      public readonly TweenSequenceElement element;

      public Effect(float startsAt, float endsAt, TweenSequenceElement element) {
        this.startsAt = startsAt;
        this.endsAt = endsAt;
        this.element = element;
      }

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      public bool isInstant => startsAt == endsAt;
    }

    public float duration { get; }
    readonly Effect[] effects;

    float _timePassed;
    public float timePassed {
      get { return _timePassed; }
      set { update(value - _timePassed); }
    }

    TweenSequence(float duration, Effect[] effects) {
      this.duration = duration;
      this.effects = effects;
    }

    public void setRelativeTimePassed(float t) =>
      update(t - _timePassed);

    public void update(float deltaTime) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return;

      var previousTime = timePassed;
      _timePassed += deltaTime;

      var forwards = deltaTime >= 0;

      if (forwards) {
        foreach (var effect in effects) {
          if (timePassed >= effect.startsAt) {
            if (effect.isInstant) {
              if (previousTime < effect.startsAt || previousTime <= 0)
                effect.element.setRelativeTimePassed(0);
            }
            else {
              if (timePassed <= effect.endsAt)
                effect.element.setRelativeTimePassed(timePassed - effect.startsAt);
              else if (previousTime <= effect.endsAt)
                effect.element.setRelativeTimePassed(effect.endsAt - effect.startsAt);
            }
          }
        }
      }
      else {
        for (var idx = effects.Length - 1; idx >= 0; idx--) {
          var effect = effects[idx];
          if (timePassed <= effect.endsAt) {
            if (effect.isInstant) {
              if (previousTime > effect.endsAt || previousTime >= duration)
                effect.element.setRelativeTimePassed(0);
            }
            else {
              if (timePassed >= effect.startsAt)
                effect.element.setRelativeTimePassed(timePassed - effect.startsAt);
              else if (previousTime >= effect.startsAt)
                effect.element.setRelativeTimePassed(effect.startsAt - effect.endsAt);
            }
          }
        }
      }
    }

    public void reset() => timePassed = 0;

    public class Builder {
      public float totalDuration { get; private set; }
      readonly List<Effect> effects = new List<Effect>();

      public TweenSequence build() => new TweenSequence(
        totalDuration,
        effects.OrderBy(_ => _.startsAt).ToArray()
      );

      public static Builder create() => new Builder();

      public Builder insert(float at, TweenSequenceElement element) {
        var endsAt = at + element.duration;
        totalDuration = Mathf.Max(totalDuration, endsAt);
        effects.Add(new Effect(at, endsAt, element));
        return this;
      }

      public Builder append(TweenSequenceElement element) =>
        insert(totalDuration, element);
    }

    public static TweenSequence parallel(params TweenSequenceElement[] elements) {
      var builder = Builder.create();
      foreach (var element in elements) 
        builder.insert(0, element);
      return builder.build();
    }

    public static TweenSequence sequential(params TweenSequenceElement[] elements) {
      var builder = Builder.create();
      foreach (var element in elements) 
        builder.append(element);
      return builder.build();
    }
  }
}