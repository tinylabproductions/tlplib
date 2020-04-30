using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class WaitForFuture {
    public static WaitForFuture<A> coroutineWait<A>(this Future<A> f) =>
      new WaitForFuture<A>(f, None._);

    public static WaitForFuture<A> coroutineWait<A>(
      this Future<A> f, Duration maxWait, TimeScale timeScale = TimeScale.Realtime
    ) => new WaitForFuture<A>(f, F.some(new MaxWait(
      timeScale.now() + maxWait.seconds, timeScale
    )));

    public struct MaxWait {
      public readonly float abortOn;
      public readonly TimeScale timeScale;

      public MaxWait(float abortOn, TimeScale timeScale) {
        this.abortOn = abortOn;
        this.timeScale = timeScale;
      }

      public bool keepWaiting => timeScale.now() < abortOn;
    }
  }

  public class WaitForFuture<A> : CustomYieldInstruction {
    public readonly Future<A> future;
    public readonly Option<WaitForFuture.MaxWait> maxWait;

    public WaitForFuture(Future<A> future, Option<WaitForFuture.MaxWait> maxWait) {
      this.future = future;
      this.maxWait = maxWait;
    }

    public override bool keepWaiting =>
      future.value.isNone && maxWait.fold(true, _ => _.keepWaiting);
  }
}
