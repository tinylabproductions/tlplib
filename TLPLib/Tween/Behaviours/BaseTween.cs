using System;
using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.Behaviours {
  abstract class BaseTween : MonoBehaviour {
    public float delay;
    public float duration = 1;
    public bool isFrom;
    public int iterations = 1;
    public GoEaseType easing;
    public GoLoopType loopType;
    GoTween tween;

    [UsedImplicitly] void Start() { runTween(); }

    private void runTween() {
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
      Go.addTween(tween);
    }

    public abstract GoTweenConfig config(GoTweenConfig cfg);

    [UsedImplicitly] void OnDestroy() {
      if (tween != null) {
        tween.destroy();
      }
    }
  }
}
