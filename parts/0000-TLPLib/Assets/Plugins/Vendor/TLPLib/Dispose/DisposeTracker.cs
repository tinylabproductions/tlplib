using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.dispose {
  public interface IDisposeTracker<in A> : IDisposable {
    void track(A a);
  }

  public class DisposeTracker<A> : IDisposeTracker<A> {
    readonly List<A> list = new List<A>();
    readonly Act<A> dispose;

    public DisposeTracker(Act<A> dispose) { this.dispose = dispose; }

    public void track(A a) => list.Add(a);
    public void track(params A[] disposables) {
      foreach (var disposable in disposables) list.Add(disposable);
    }

    public void Dispose() {
      foreach (var a in list) dispose(a);
      list.Clear();
    }
  }

  public class DisposableTracker : DisposeTracker<IDisposable> {
    static readonly Act<IDisposable> dispose = d => d.Dispose();

    public DisposableTracker() : base(dispose) {}
  }
}