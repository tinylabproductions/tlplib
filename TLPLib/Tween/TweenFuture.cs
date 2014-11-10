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

  class TweenFutureCompleted : ITweenFuture {
    public Option<Unit> pureValue { get { return F.some(F.unit); } }
    public Option<Try<Unit>> value { get { return pureValue.map(F.scs); } }

    public CancellationToken onComplete(Act<Try<Unit>> action) {
      action(F.scs(F.unit));
      return Future.FinishedCancellationToken.instance;
    }

    public CancellationToken onSuccess(Act<Unit> action) {
      action(F.unit);
      return Future.FinishedCancellationToken.instance;
    }

    public CancellationToken onFailure(Act<Exception> action) {
      return Future.FinishedCancellationToken.instance;
    }

    public Future<Unit> tapComplete(Act<Try<Unit>> action) { return this; }
    public Future<Unit> tapSuccess(Act<Unit> action) { return this; }
    public Future<Unit> tapFailure(Act<Exception> action) { return this; }

    public void destroy() {}
  }
}
