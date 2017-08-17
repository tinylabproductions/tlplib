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

  public interface TweenSequenceElement {
    float duration { get; }
    void setRelativeTimePassed(float t, bool playingForwards);
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

    // ReSharper disable CompareOfFloatsByEqualityOperator
    public bool atZero => _timePassed == 0;
    public bool atDuration => _timePassed == duration;
    // ReSharper restore CompareOfFloatsByEqualityOperator

    TweenSequence(float duration, Effect[] effects) {
      this.duration = duration;
      this.effects = effects;
    }

    public void setRelativeTimePassed(float t, bool playingForwards) =>
      update(t - _timePassed);

    public void update(float deltaTime) {
      var previousTime = timePassed;
      _timePassed = Mathf.Clamp(timePassed + deltaTime, 0, duration);

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (previousTime == _timePassed) return;

      var forwards = deltaTime >= 0;

      if (forwards) {
        foreach (var effect in effects) {
          if (timePassed >= effect.startsAt) {
            if (effect.isInstant) {
              if (previousTime < effect.startsAt || previousTime <= 0)
                effect.element.setRelativeTimePassed(0, forwards);
            }
            else {
              if (timePassed <= effect.endsAt)
                effect.element.setRelativeTimePassed(timePassed - effect.startsAt, forwards);
              else if (previousTime <= effect.endsAt)
                effect.element.setRelativeTimePassed(effect.endsAt - effect.startsAt, forwards);
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
                effect.element.setRelativeTimePassed(0, forwards);
            }
            else {
              if (timePassed >= effect.startsAt)
                effect.element.setRelativeTimePassed(timePassed - effect.startsAt, forwards);
              else if (previousTime >= effect.startsAt)
                effect.element.setRelativeTimePassed(effect.startsAt - effect.endsAt, forwards);
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

      /// <summary>Inserts element into the sequence at specific time.</summary>
      public Builder insert(float at, TweenSequenceElement element) {
        var endsAt = at + element.duration;
        totalDuration = Mathf.Max(totalDuration, endsAt);
        effects.Add(new Effect(at, endsAt, element));
        return this;
      }

      /// <see cref="insert(float,TweenSequenceElement)"/>
      /// <returns>Time when the given element will end.</returns>
      public float insert2(float at, TweenSequenceElement element) {
        insert(at, element);
        return at + element.duration;
      }

      /// <see cref="insert(float,TweenSequenceElement)"/>
      /// <param name="at"></param>
      /// <param name="element"></param>
      /// <param name="elementEndsAt">Time when the given element will end.</param>
      public Builder insert(float at, TweenSequenceElement element, out float elementEndsAt) {
        insert(at, element);
        elementEndsAt = at + element.duration;
        return this;
      }

      public Builder append(TweenSequenceElement element) =>
        insert(totalDuration, element);

      public float append2(TweenSequenceElement element) =>
        insert2(totalDuration, element);

      public Builder append(TweenSequenceElement element, out float elementEndsAt) =>
        insert(totalDuration, element, out elementEndsAt);
    }

    public static Builder parallel(params TweenSequenceElement[] elements) {
      var builder = Builder.create();
      foreach (var element in elements) 
        builder.insert(0, element);
      return builder;
    }

    public static Builder sequential(params TweenSequenceElement[] elements) {
      var builder = Builder.create();
      foreach (var element in elements) 
        builder.append(element);
      return builder;
    }
  }
}