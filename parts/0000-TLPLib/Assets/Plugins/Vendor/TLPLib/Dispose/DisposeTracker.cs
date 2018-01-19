using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.dispose {
  public interface IDisposableTracker : IDisposable {
    void track(IDisposable a);
  }

  public class DisposableTracker : IDisposableTracker {
    readonly List<IDisposable> list = new List<IDisposable>();

    public void track(IDisposable a) => list.Add(a);
    public int count => list.Count;

    public void Dispose() {
      foreach (var a in list) a.Dispose();
      list.Clear();
      list.Capacity = 0;
    }
  }

  public class ClosureDisposableTracker : IDisposableTracker {
    readonly IDisposableTracker backing;
    readonly object reference;

    public ClosureDisposableTracker(IDisposableTracker backing, object reference) {
      this.backing = backing;
      this.reference = reference;
    }

    public void Dispose() => backing.Dispose();
    public void track(IDisposable a) => backing.track(new ClosureDisposable(a, reference));
  }
  
  public class ClosureDisposable : IDisposable {
    [CanBeNull] IDisposable original;
    // ReSharper disable once NotAccessedField.Local
    [CanBeNull] object reference;

    public ClosureDisposable(IDisposable original, object reference) {
      this.original = original;
      this.reference = reference;
    }

    public void Dispose() {
      if (original == null) return;
      original.Dispose();
      original = null;
      reference = null;
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