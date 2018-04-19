using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  /// <summary>
  /// Anything that can be added into a <see cref="ITweenSequence"/>.
  /// </summary>
  public interface TweenSequenceElement {
    float duration { get; }
    void setRelativeTimePassed(float t, bool playingForwards);
  }

  /// <summary>
  /// A sequence of <see cref="TweenSequenceElement"/> arranged in time.
  /// </summary>
  public interface ITweenSequence : TweenSequenceElement {
    float timePassed { get; set; }
  }

  public class TweenSequence : ITweenSequence {
    struct Effect {
      public readonly float startsAt, endsAt;
      public readonly TweenSequenceElement element;

      public Effect(float startsAt, float endsAt, TweenSequenceElement element) {
        this.startsAt = startsAt;
        this.endsAt = endsAt;
        this.element = element;
      }
    }

    public float duration { get; }
    readonly Effect[] effects;

    bool lastDirectionWasForwards;

    float _timePassed;
    public float timePassed {
      get => _timePassed;
      set {
        var diff = value - _timePassed;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (diff == 0f) return;

        var playingForwards = Mathf.Sign(diff) > 0;
        setRelativeTimePassed(value, playingForwards);
      }
    }

    TweenSequence(float duration, Effect[] effects) {
      this.duration = duration;
      this.effects = effects;
    }

    // optimized for minimal allocations
    public static TweenSequence single(TweenSequenceElement sequence, float delay = 0) =>
      new TweenSequence(
        sequence.duration + delay,
        new []{ new Effect(delay, sequence.duration + delay, sequence) }
      );

    public void setRelativeTimePassed(float t, bool playingForwards) {
      var previousTime = _timePassed;
      _timePassed = Mathf.Clamp(t, 0, duration);

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (previousTime == _timePassed && playingForwards == lastDirectionWasForwards) return;

      var directionChanged = playingForwards != lastDirectionWasForwards;

      if (playingForwards) {
        foreach (var effect in effects) {
          if (timePassed >= effect.startsAt && previousTime <= effect.endsAt) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (previousTime == effect.endsAt) {
              if (directionChanged) effect.element.setRelativeTimePassed(effect.endsAt - effect.startsAt, playingForwards);
            }
            else if (timePassed <= effect.endsAt)
              effect.element.setRelativeTimePassed(timePassed - effect.startsAt, playingForwards);
            else
              effect.element.setRelativeTimePassed(effect.endsAt - effect.startsAt, playingForwards);
          }
        }
      }
      else {
        for (var idx = effects.Length - 1; idx >= 0; idx--) {
          var effect = effects[idx];
          if (timePassed <= effect.endsAt && previousTime >= effect.startsAt) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (previousTime == effect.startsAt) {
              if (directionChanged) effect.element.setRelativeTimePassed(effect.startsAt - effect.endsAt, playingForwards);
            }
            else if (timePassed >= effect.startsAt)
              effect.element.setRelativeTimePassed(timePassed - effect.startsAt, playingForwards);
            else
              effect.element.setRelativeTimePassed(effect.startsAt - effect.endsAt, playingForwards);
          }
        }
      }
      lastDirectionWasForwards = playingForwards;
    }

    public class Builder {
      [PublicAPI] public float totalDuration { get; private set; }
      readonly List<Effect> effects = new List<Effect>();

      public TweenSequence build() => new TweenSequence(
        totalDuration,
        effects.OrderBy(_ => _.startsAt).ToArray()
      );

      public static Builder create() => new Builder();

      /// <summary>Inserts element into the sequence at specific time.</summary>
      [PublicAPI]
      public Builder insert(float at, TweenSequenceElement element) {
        var endsAt = at + element.duration;
        totalDuration = Mathf.Max(totalDuration, endsAt);
        effects.Add(new Effect(at, endsAt, element));
        return this;
      }

      /// <see cref="insert(float,TweenSequenceElement)"/>
      /// <returns>Time when the given element will end.</returns>
      [PublicAPI]
      public float insert2(float at, TweenSequenceElement element) {
        insert(at, element);
        return at + element.duration;
      }

      /// <see cref="insert(float,TweenSequenceElement)"/>
      /// <param name="at"></param>
      /// <param name="element"></param>
      /// <param name="elementEndsAt">Time when the given element will end.</param>
      [PublicAPI]
      public Builder insert(float at, TweenSequenceElement element, out float elementEndsAt) {
        insert(at, element);
        elementEndsAt = at + element.duration;
        return this;
      }

      [PublicAPI]
      public Builder append(TweenSequenceElement element) =>
        insert(totalDuration, element);

      [PublicAPI]
      public float append2(TweenSequenceElement element) =>
        insert2(totalDuration, element);

      [PublicAPI]
      public Builder append(TweenSequenceElement element, out float elementEndsAt) =>
        insert(totalDuration, element, out elementEndsAt);
    }

    [PublicAPI] 
    public static Builder parallelEnumerable(IEnumerable<TweenSequenceElement> elements) {
      var builder = Builder.create();
      foreach (var element in elements)
        builder.insert(0, element);
      return builder;
    }

    [PublicAPI] 
    public static Builder parallel(params TweenSequenceElement[] elements) =>
      parallelEnumerable(elements);

    [PublicAPI] 
    public static Builder sequentialEnumerable(IEnumerable<TweenSequenceElement> elements) {
      var builder = Builder.create();
      foreach (var element in elements)
        builder.append(element);
      return builder;
    }

    [PublicAPI]
    public static Builder sequential(params TweenSequenceElement[] elements) =>
      sequentialEnumerable(elements);
  }

  // TODO: this fires forwards events, when playing from the end. We should fix this.
  class TweenSequenceReversed : ITweenSequence {
    public readonly ITweenSequence original;

    public TweenSequenceReversed(ITweenSequence original) { this.original = original; }

    public float duration => original.duration;

    public void setRelativeTimePassed(float t, bool playingForwards) =>
      original.setRelativeTimePassed(original.duration - t, !playingForwards);

    public float timePassed {
      get => original.duration - original.timePassed;
      set => original.timePassed = original.duration - value;
    }
  }

  public static class TweenSequenceExts {
    // ReSharper disable CompareOfFloatsByEqualityOperator
    public static bool isAtZero(this ITweenSequence ts) => ts.timePassed == 0;
    public static bool isAtDuration(this ITweenSequence ts) => ts.timePassed == ts.duration;
    // ReSharper restore CompareOfFloatsByEqualityOperator

    public static ITweenSequence reversed(this ITweenSequence ts) {
      foreach (var r in F.opt(ts as TweenSequenceReversed))
        return r.original;
      return new TweenSequenceReversed(ts);
    }

    public static TweenSequence.Builder singleBuilder(this TweenSequenceElement element) {
      var builder = TweenSequence.Builder.create();
      builder.append(element);
      return builder;
    }

    public static void update(this ITweenSequence element, float deltaTime) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return;

      var directionForwards = Mathf.Sign(deltaTime) >= 0;
      element.setRelativeTimePassed(element.timePassed + deltaTime , directionForwards);
    }

    public static TweenSequence single(this TweenSequenceElement element) =>
      TweenSequence.single(element);
  }
}