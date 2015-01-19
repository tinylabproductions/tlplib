using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  public abstract class BaseTween : MonoBehaviour {
    public bool playOnStart = true;
    public float delay;
    public float duration = 1;
    public bool isFrom;
    public int iterations = 1;
    public GoEaseType easing;
    public GoLoopType loopType;
    public GoTween tween;

    [UsedImplicitly]
    void Awake() {
      tween = new GoTween(
        transform, 
        duration,
        config(
          new GoTweenConfig().
          setDelay(delay).
          setEaseType(easing).
          setIterations(iterations, loopType).
          tap(_ => _.isFrom = isFrom)
        ));
      if (!playOnStart) {
        tween.autoRemoveOnComplete = false;
      }
      tween.pause();
      Go.addTween(tween);
    }

    [UsedImplicitly]
    void Start() {
      if (playOnStart) {
        play();
      }
    }

    public void play() {
      tween.play();
    }

    public void restart() {
      tween.restart();
    }

    public void playForward() {
      tween.playForward();
    }

    public void playBackwards() {
      tween.playBackwards();
    }

    public abstract GoTweenConfig config(GoTweenConfig cfg);

    [UsedImplicitly] void OnDestroy() {
      if (tween != null) {
        tween.destroy();
      }
    }
  }
}
