using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.dispose {
  public interface IDisposableTracker {
    void track(IDisposable a);
  }

  public class DisposableTracker : IDisposableTracker {
    readonly List<IDisposable> list = new List<IDisposable>();

    public void track(IDisposable a) => list.Add(a);

    public void Dispose() {
      foreach (var a in list) a.Dispose();
      list.Clear();
      list.Capacity = 0;
    }
  }
}