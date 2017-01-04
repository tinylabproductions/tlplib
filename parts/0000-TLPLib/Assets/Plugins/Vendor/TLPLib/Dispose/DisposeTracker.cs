using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.dispose {
  public interface IDisposeTracker<in A> : IDisposable {
    void track(A a);
  }

  public class DisposeTracker<A> : IDisposeTracker<A> {
    readonly SList4<A> list = new SList4<A>();
    readonly Act<A> dispose;

    public DisposeTracker(Act<A> dispose) { this.dispose = dispose; }

    public void track(A a) => list.Add(a);

    public void Dispose() {
      foreach (var a in list) dispose(a);
      list.clear();
    }
  }

  public class DisposableTracker : DisposeTracker<IDisposable> {
    static readonly Act<IDisposable> dispose = d => d.Dispose();

    public DisposableTracker() : base(dispose) {}
  }
}