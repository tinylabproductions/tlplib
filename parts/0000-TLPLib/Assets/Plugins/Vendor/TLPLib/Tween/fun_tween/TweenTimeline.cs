using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  /// <summary>
  /// Anything that can be added into a <see cref="ITweenTimeline"/>.
  /// </summary>
  public interface TweenTimelineElement {
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
    /// <returns> true if object which is being tweened is not destroyed</returns>
    bool setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, 
      bool applyEffectsForRelativeTweens
    );

    // Not an option for performance.
    bool asApplyStateAt(out IApplyStateAt applyStateAt);
  }

  public interface IApplyStateAt {
    /// <summary>
    /// Applies absolute <see cref="Tweener{A,T}"/>s at a given time.
    ///
    /// Useful to force object states into those, which would be if tween was playing at
    /// some time, for example - 0s.
    /// </summary>
    bool applyStateAt(float time);
  }

  /// <summary>
  /// <see cref="TweenTimelineElement"/>s arranged in time.
  /// </summary>
  public interface ITweenTimeline : TweenTimelineElement, IApplyStateAt {
    /// <summary>Calls <see cref="setTimePassed"/> applying effects for relative tweens.</summary>
    float timePassed { get; }
    
    /// <see cref="TweenTimelineElement.setRelativeTimePassed"/>
    bool setTimePassed(float timePassed, bool applyEffectsForRelativeTweens);
  }

  public class TweenTimeline : ITweenTimeline {
    struct Effect {
      public readonly float startsAt, endsAt;
      public readonly TweenTimelineElement element;

      public readonly float duration;
      public float relativize(float timePassed) => timePassed - startsAt;

      public Effect(float startsAt, float endsAt, TweenTimelineElement element) {
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

    public bool setTimePassed(float value, bool applyEffectsForRelativeTweens) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (_timePassed == value) return true;
      var playingForwards = value >= _timePassed; 
        
      return setRelativeTimePassed(
        previousTimePassed: _timePassed, timePassed: value, playingForwards: playingForwards, 
        applyEffectsForRelativeTweens: applyEffectsForRelativeTweens
      );
    }

    TweenTimeline(float duration, Effect[] effects) {
      this.duration = duration;
      this.effects = effects;
    }

    // optimized for minimal allocations
    [PublicAPI]
    public static TweenTimeline single(TweenTimelineElement timeline, float delay = 0) =>
      new TweenTimeline(
        timeline.duration + delay,
        new []{ new Effect(delay, timeline.duration + delay, timeline) }
      );

    public bool setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) {
      bool noErrors = true;
      _timePassed = Mathf.Clamp(timePassed, 0, duration);

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (previousTimePassed == _timePassed && playingForwards == lastDirectionWasForwards) return true;

      var directionChanged = playingForwards != lastDirectionWasForwards;

      if (playingForwards) {
        foreach (var effect in effects) {
          if (timePassed >= effect.startsAt && previousTimePassed <= effect.endsAt) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (previousTimePassed == effect.endsAt) {
              if (directionChanged) {
                if (!effect.element.setRelativeTimePassed(
                  effect.duration, effect.duration, true, applyEffectsForRelativeTweens
                )) noErrors = false;
              }
            }
            else {
              float t(float x) => x <= effect.endsAt ? effect.relativize(x) : effect.duration;
              if (!effect.element.setRelativeTimePassed(
                t(previousTimePassed), t(timePassed), true, applyEffectsForRelativeTweens
              )) noErrors = false;
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
              if (directionChanged) {
                if (!effect.element.setRelativeTimePassed(0, 0, false, applyEffectsForRelativeTweens)) noErrors = false;
              }
            }
            else {
              float t(float x) => x >= effect.startsAt ? effect.relativize(x) : 0;
              if (!effect.element.setRelativeTimePassed(
                t(previousTimePassed), t(timePassed), false, applyEffectsForRelativeTweens
              )) noErrors = false;
            }
          }
        }
      }
      lastDirectionWasForwards = playingForwards;
      return noErrors;
    }

    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = this;
      return true;
    }

    public bool applyStateAt(float time) {
      var noErrors = true;
      foreach (var effect in effects) {
        if (
          time >= effect.startsAt  
          && time <= effect.endsAt 
          && effect.element.asApplyStateAt(out var stateEffect)
        ) {
          if (!stateEffect.applyStateAt(effect.relativize(time))) noErrors = false;
        }
      }

      return noErrors;
    }
    
    public class Builder {
      [PublicAPI] public float totalDuration { get; private set; }
      readonly List<Effect> effects = new List<Effect>();

      public TweenTimeline build() => new TweenTimeline(
        totalDuration,
        effects.OrderBy(_ => _.startsAt).ToArray()
      );

      public static Builder create() => new Builder();

      /// <summary>Inserts element into the sequence at specific time.</summary>
      [PublicAPI]
      public Builder insert(float at, TweenTimelineElement element) {
        var endsAt = at + element.duration;
        totalDuration = Mathf.Max(totalDuration, endsAt);
        effects.Add(new Effect(at, endsAt, element));
        return this;
      }

      /// <see cref="insert(float,TweenTimelineElement)"/>
      /// <returns>Time when the given element will end.</returns>
      [PublicAPI]
      public float insert2(float at, TweenTimelineElement element) {
        insert(at, element);
        return at + element.duration;
      }

      /// <see cref="insert(float,TweenTimelineElement)"/>
      /// <param name="at"></param>
      /// <param name="element"></param>
      /// <param name="elementEndsAt">Time when the given element will end.</param>
      [PublicAPI]
      public Builder insert(float at, TweenTimelineElement element, out float elementEndsAt) {
        insert(at, element);
        elementEndsAt = at + element.duration;
        return this;
      }

      [PublicAPI]
      public Builder append(TweenTimelineElement element) =>
        insert(totalDuration, element);

      [PublicAPI]
      public Builder appendWithDelay(Duration delay, TweenTimelineElement element) =>
        insert(totalDuration + delay.seconds, element);

      [PublicAPI]
      public Builder append(Option<TweenTimelineElement> element) =>
        element.isSome ? append(element.__unsafeGetValue) : this;

      [PublicAPI]
      public float append2(TweenTimelineElement element) =>
        insert2(totalDuration, element);

      [PublicAPI]
      public Builder append(TweenTimelineElement element, out float elementEndsAt) =>
        insert(totalDuration, element, out elementEndsAt);
      
      [PublicAPI] 
      public Builder appendSequentially(IEnumerable<TweenTimelineElement> elements) {
        foreach (var element in elements)
          append(element);
        return this;
      }
    }

    [PublicAPI] 
    public static Builder parallelEnumerable(IEnumerable<TweenTimelineElement> elements) {
      var builder = Builder.create();
      foreach (var element in elements)
        builder.insert(0, element);
      return builder;
    }

    [PublicAPI] 
    public static Builder parallel(params TweenTimelineElement[] elements) =>
      parallelEnumerable(elements);

    [PublicAPI] 
    public static Builder sequentialEnumerable(IEnumerable<TweenTimelineElement> elements) {
      var builder = Builder.create();
      foreach (var element in elements)
        builder.append(element);
      return builder;
    }

    [PublicAPI]
    public static Builder sequential(params TweenTimelineElement[] elements) =>
      sequentialEnumerable(elements);
    
    [PublicAPI] public static Builder builder() => Builder.create();
  }

  // TODO: this fires forwards events, when playing from the end. We should fix this.
  class TweenTimelineReversed : ITweenTimeline {
    public readonly ITweenTimeline original;

    public TweenTimelineReversed(ITweenTimeline original) { this.original = original; }

    public float duration => original.duration;

    public bool setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) =>
      original.setRelativeTimePassed(
        previousTimePassed: original.duration - previousTimePassed,
        timePassed: original.duration - timePassed, 
        playingForwards: !playingForwards,
        applyEffectsForRelativeTweens: applyEffectsForRelativeTweens
      );

    public bool asApplyStateAt(out IApplyStateAt applyStateAt) => original.asApplyStateAt(out applyStateAt);
    public bool applyStateAt(float time) => original.applyStateAt(original.duration - time);

    public bool setTimePassed(float timePassed, bool applyEffectsForRelativeTweens) =>
      original.setTimePassed(original.duration - timePassed, applyEffectsForRelativeTweens);

    public float timePassed => original.duration - original.timePassed;
  }

  public static class TweenSequenceExts {
    // ReSharper disable CompareOfFloatsByEqualityOperator
    public static bool isAtZero(this ITweenTimeline ts) => ts.timePassed == 0;
    public static bool isAtDuration(this ITweenTimeline ts) => ts.timePassed == ts.duration;
    // ReSharper restore CompareOfFloatsByEqualityOperator

    [PublicAPI]
    public static ITweenTimeline reversed(this ITweenTimeline ts) {
      foreach (var r in F.opt(ts as TweenTimelineReversed))
        return r.original;
      return new TweenTimelineReversed(ts);
    }

    [PublicAPI]
    public static TweenTimeline.Builder singleBuilder(this TweenTimelineElement element) {
      var builder = TweenTimeline.Builder.create();
      builder.append(element);
      return builder;
    }

    public static bool update(this ITweenTimeline element, float deltaTime) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (deltaTime == 0) return true;

      return element.setTimePassed(element.timePassed + deltaTime, true);
    }

    [PublicAPI] 
    public static TweenTimeline single(this TweenTimelineElement element) =>
      TweenTimeline.single(element);
  }
}