using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Tween {
  /* Future which completes when the tween completes or is destroyed. */
  public interface ITweenFuture : Future<Unit> {
    void destroy();
  }

  public delegate AbstractGoTween TweenFutureCreator(GoTweenConfig config);

  public static class TweenFuture {
    public static ITweenFuture a(AbstractGoTween tween) {
      return new TweenFutureImpl(tween);
    }

    public static ITweenFuture a(TweenFutureCreator creator) {
      return new TweenFutureImpl(creator);
    }

    public static readonly ITweenFuture noTween = new TweenFutureCompleted();
  }

  class TweenFutureImpl : FutureImpl<Unit>, ITweenFuture {
    private readonly AbstractGoTween tween;

    internal TweenFutureImpl(AbstractGoTween tween) {
      this.tween = tween;
      /* Don't you love when there are no appropriate events that can be multi subscribed? */
      ASync.EveryFrame(() => {
        switch (tween.state) {
          case GoTweenState.Complete:
          case GoTweenState.Destroyed:
            Log.debug(string.Format(
              "Tween {0} completed in future {1}, state = {2}",
              tween.debugObj(), this.debugObj(), tween.state
            ));
            completeSuccess(F.unit);
            return false;
          default:
            return true;
        }
      });
    }

    internal TweenFutureImpl(TweenFutureCreator creator) 
    : this(creator(new GoTweenConfig().startPaused())) {
      tween.play();
    }

    public void destroy() { tween.destroy(); }
  }

  class TweenFutureCompleted : FutureImpl<Unit>, ITweenFuture {
    public TweenFutureCompleted() { completeSuccess(F.unit); }

    public void destroy() {}
  }
}
