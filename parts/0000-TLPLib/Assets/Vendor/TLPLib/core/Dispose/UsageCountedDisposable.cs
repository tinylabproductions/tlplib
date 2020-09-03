using System;
using pzd.lib.dispose;

namespace com.tinylabproductions.TLPLib.Dispose {
  public static class UsageCountedDisposable {
    public static UsageCountedDisposable<A> a<A>(A value, Action<A> onDispose) =>
      new UsageCountedDisposable<A>(value, onDispose);
  }

  /// <summary>Disposable that automatically disposes underlying resource when all the users
  /// dispose it.</summary>
  public class UsageCountedDisposable<A> {
    readonly A value;
    readonly Action<A> onDispose;

    uint totalUsers;
    bool disposed;

    public UsageCountedDisposable(A value, Action<A> onDispose) {
      this.value = value;
      this.onDispose = onDispose;
    }

    public Disposable<A> use() {
      if (disposed) throw new IllegalStateException(
        $"Can't {nameof(use)}() a disposed resource '{value}'!"
      );

      totalUsers++;
      return new Disposable<A>(value, _ => {
        totalUsers--;
        if (totalUsers == 0) {
          onDispose(value);
          disposed = true;
        }
      });
    }
  }
}