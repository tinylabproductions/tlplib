using System.Collections;
using pzd.lib.concurrent;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /// <summary>Allows using <see cref="Future{A}"/> as a Unity coroutine.</summary>
  public sealed class FutureEnumerator<A> : IEnumerator {
    public readonly Future<A> future;

    public FutureEnumerator(Future<A> future) => this.future = future;

    /// <summary>Returning null here makes Unity schedule next check for the next frame.</summary>
    public object Current => null;
    /// <summary>Unity invokes this to determine whether it should consider the coroutine finished.</summary>
    public bool MoveNext() => !future.isCompleted;
    public void Reset() {}
  }
}