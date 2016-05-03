using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class WaitForFuture {
    public static WaitForFuture<A> a<A>(Future<A> f) { return new WaitForFuture<A>(f); }
    public static WaitForFuture<A> coroutineWait<A>(this Future<A> f) { return new WaitForFuture<A>(f); }
  }

  public class WaitForFuture<A> : CustomYieldInstruction {
    public readonly Future<A> future;

    public WaitForFuture(Future<A> future) { this.future = future; }

    public override bool keepWaiting => future.value.isEmpty;
  }
}
