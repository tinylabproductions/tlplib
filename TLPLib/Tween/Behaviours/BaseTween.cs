using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  public abstract class BaseTween : MonoBehaviour {
    public bool playOnStart = true;
    public float delay;
    public float duration = 1;
    public bool isFrom;
    public int iterations = 1;
    public GoEaseType easing;
    public GoLoopType loopType;
    GoTween _tween;
    public GoTween tween { get { 
      if (_tween == null) init();
      return _tween;
    } }

    [UsedImplicitly]
    void Awake() {
      if (_tween == null)
        init();
    }

    void init() {
      _tween = new GoTween(
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
        _tween.autoRemoveOnComplete = false;
      }
      _tween.pause();
      Go.addTween(_tween);
    }

    public void editorRefresh() {
      if (_tween != null) {
        _tween.destroy();
      }
      init();
      play();
    }

    [UsedImplicitly]
    void Start() {
      if (playOnStart) {
        play();
      }
    }

    public void play() {
      _tween.play();
    }

    public void restart() {
      _tween.restart();
    }

    public void playForward() {
      _tween.playForward();
    }

    public void playBackwards() {
      _tween.playBackwards();
    }

    public abstract GoTweenConfig config(GoTweenConfig cfg);

    [UsedImplicitly] void OnDestroy() {
      if (_tween != null) {
        _tween.destroy();
      }
    }
  }
}
