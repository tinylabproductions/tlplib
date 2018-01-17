using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.dispose {
  public interface IDisposableTracker : IDisposable {
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

  /// <summary>
  /// Used when we are sure that we never want to clean the subscription automatically
  /// (for example in Observable operations). 
  /// </summary>
  public class NoOpDisposableTracker : IDisposableTracker {
    public static readonly NoOpDisposableTracker instance = new NoOpDisposableTracker();
    NoOpDisposableTracker() {}
    
    public void track(IDisposable a) {}
    public void Dispose() {}
  }
}