using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  /// <summary>
  /// Knows how to change state of some property <see cref="A"/> on <see cref="T"/>.
  ///
  /// For example how to change <see cref="Vector3"/> of <see cref="Transform.position"/>.
  /// </summary>
  public class Tweener<A, T> : TweenTimelineElement, IApplyStateAt {
    [PublicAPI] public float duration => tween.duration;

    [PublicAPI] public readonly Tween<A> tween;
    [PublicAPI] public readonly T t;
    [PublicAPI] public readonly TweenMutator<A, T> changeState;

    public Tweener(Tween<A> tween, T t, TweenMutator<A, T> changeState) {
      this.tween = tween;
      this.t = t;
      this.changeState = changeState;
    }

    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens, 
      bool exitTween, bool isReset
    ) {
      if (applyEffectsForRelativeTweens || !tween.isRelative) {
        changeState(tween.eval(previousTimePassed, timePassed, playingForwards), t, tween.isRelative);
      }
    }

    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = this;
      return true;
    }

    public void applyStateAt(float time) {
      // We do not apply relative tween states, because they do not make sense in a fixed time point.
      if (!tween.isRelative) {
        changeState(tween.evalAt(time), t, false);
      }
    }

    public override string ToString() {
      var relativeS = tween.isRelative ? "relative " : "";
      return $"{nameof(Tweener<A, T>)}[{relativeS}on {t}, {tween}]";
    }
  }
}