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
    
    /// <summary>
    /// Sets how much time has passed, relative to elements duration.
    /// 
    /// Must be [0, duration].
    /// </summary>
    /// 
    /// <param name="previousTimePassed"></param>
    /// <param name="timePassed"></param>
    /// <param name="playingForwards"></param>
    /// <param name="applyEffectsForRelativeTweens">
    /// Should we run effects for relative tweens when setting the time passed?
    /// 
    /// Usually you want to run them, but one case when you do not want effects to happen
    /// is rewinding - you want for the object to stay in place even though the logical time
    /// of the sequence changes to 0 or total duration. 
    /// </param>
    void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, 
      bool applyEffectsForRelativeTweens
    );
  }

  /// <summary>
  /// A sequence of <see cref="TweenSequenceElement"/> arranged in time.
  /// </summary>
  public interface ITweenSequence : TweenSequenceElement {
    /// <summary>Calls <see cref="setTimePassed"/> applying effects for relative tweens.</summary>
    float timePassed { get; set; }
    
    /// <see cref="TweenSequenceElement.setRelativeTimePassed"/>
    void setTimePassed(float timePassed, bool applyEffectsForRelativeTweens);
  }

  public class TweenSequence : ITweenSequence {
    struct Effect {
      public readonly float startsAt, endsAt;
      public readonly TweenSequenceElement element;

      public readonly float duration;
      public float relativize(float timePassed) => timePassed - startsAt;

      public Effect(float startsAt, float endsAt, TweenSequenceElement element) {
        this.startsAt = startsAt;
        this.endsAt = endsAt;
        this.element = element;
        duration = endsAt - startsAt;
      }
    }

    public float duration { get; }
    readonly Effect[] effects;

    bool lastDirectionWasForwards;

    float _timePassed;
    public float timePassed {
      get => _timePassed;
      set => setTimePassed(value, true);
    }

    public void setTimePassed(float value, bool applyEffectsForRelativeTweens) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (_timePassed == value) return;
      var playingForwards = value >= _timePassed; 
        
      setRelativeTimePassed(
        previousTimePassed: _timePassed, timePassed: value, playingForwards: playingForwards, 
        applyEffectsForRelativeTweens: applyEffectsForRelativeTweens
      );
    }

    TweenSequence(float duration, Effect[] effects) {
      this.duration = duration;
      this.effects = effects;
    }

    // optimized for minimal allocations
    [PublicAPI]
    public static TweenSequence single(TweenSequenceElement sequence, float delay = 0) =>
      new TweenSequence(
        sequence.duration + delay,
        new []{ new Effect(delay, sequence.duration + delay, sequence) }
      );

    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) {
      _timePassed = Mathf.Clamp(timePassed, 0, duration);

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (previousTimePassed == _timePassed && playingForwards == lastDirectionWasForwards) return;

      var directionChanged = playingForwards != lastDirectionWasForwards;

      if (playingForwards) {
        foreach (var effect in effects) {
          if (timePassed >= effect.startsAt && previousTimePassed <= effect.endsAt) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (previousTimePassed == effect.endsAt) {
              if (directionChanged)
                effect.element.setRelativeTimePassed(
                  effect.duration, effect.duration, true, applyEffectsForRelativeTweens
                );
            }
            else {
              float t(float x) => x <= effect.endsAt ? effect.relativize(x) : effect.duration;
              effect.element.setRelativeTimePassed(
                t(previousTimePassed), t(timePassed), true, applyEffectsForRelativeTweens
              );
            }
          }
        }
      }
      else {
        for (var idx = effects.Length - 1; idx >= 0; idx--) {
          var effect = effects[idx];
          if (timePassed <= effect.endsAt && previousTimePassed >= effect.startsAt) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (previousTimePassed == effect.startsAt) {
              if (directionChanged) effect.element.setRelativeTimePassed(0, 0, false, applyEffectsForRelativeTweens);
            }
            else {
              float t(float x) => x >= effect.startsAt ? effect.relativize(x) : 0;
              effect.element.setRelativeTimePassed(
                t(previousTimePassed), t(timePassed), false, applyEffectsForRelativeTweens
              );
            }
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

    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) =>
      original.setRelativeTimePassed(
        previousTimePassed: original.duration - previousTimePassed,
        timePassed: original.duration - timePassed, 
        playingForwards: !playingForwards,
        applyEffectsForRelativeTweens: applyEffectsForRelativeTweens
      );

    public void setTimePassed(float timePassed, bool applyEffectsForRelativeTweens) =>
      original.setTimePassed(original.duration - timePassed, applyEffectsForRelativeTweens);

    public float timePassed {
      get => original.duration - original.timePassed;
      set => setTimePassed(value, true);
    }
  }

  public static class TweenSequenceExts {
    // ReSharper disable CompareOfFloatsByEqualityOperator
    public static bool isAtZero(this ITweenSequence ts) => ts.timePassed == 0;
    public static bool isAtDuration(this ITweenSequence ts) => ts.timePassed == ts.duration;
    // ReSharper restore CompareOfFloatsByEqualityOperator

    [PublicAPI]
    public static ITweenSequence reversed(this ITweenSequence ts) {
      foreach (var r in F.opt(ts as TweenSequenceReversed))
        return r.original;
      return new TweenSequenceReversed(ts);
    }

    [PublicAPI]
    public static TweenSequence.Builder singleBuilder(this TweenSequenceElement element) {
      var builder = TweenSequence.Builder.create();
      builder.append(element);
      return builder;
    }

    public static void update(this ITweenSequence element, float deltaTime) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return;

      element.timePassed += deltaTime;
    }

    [PublicAPI] 
    public static TweenSequence single(this TweenSequenceElement element) =>
      TweenSequence.single(element);
  }
}