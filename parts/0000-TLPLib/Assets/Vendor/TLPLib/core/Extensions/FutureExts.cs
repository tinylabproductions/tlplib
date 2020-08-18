using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FutureExts {
    /// <summary>Allows using <see cref="Future{A}"/> as a Unity coroutine.</summary>
    ///
    /// <see cref="FutureEnumerator{A}"/>
    public static IEnumerator toEnumerator<A>(this Future<A> future) => new FutureEnumerator<A>(future);
  }
}