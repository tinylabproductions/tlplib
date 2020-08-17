#if UNITY_EDITOR
using System;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [Singleton, PublicAPI] public sealed partial class EditorTimeContext : ITimeContext {
    public TimeSpan passedSinceStartup => TimeSpan.FromSeconds(EditorApplication.timeSinceStartup);
    
    public ICoroutine after(TimeSpan duration, Action act, string name = null) => 
      new EditorCoroutine(duration, act);

    class EditorCoroutine : ICoroutine {
      public event Action onFinish;
      public bool finished { get; private set; }
      
      readonly TimeSpan duration;
      readonly Action action;
      readonly double startedAt;
      
      public EditorCoroutine(TimeSpan duration, Action action) {
        this.duration = duration;
        this.action = action;
        startedAt = EditorApplication.timeSinceStartup;
        EditorApplication.update += onUpdate;
      }

      void onUpdate() {
        if (EditorApplication.timeSinceStartup >= startedAt + duration.TotalSeconds) {
          action();
          onFinish?.Invoke();
          Dispose();
        }
      }

      public void Dispose() {
        if (finished) return;
        EditorApplication.update -= onUpdate;
        finished = true;
      }

      public void Reset() {}
      public object Current => null;
      public bool MoveNext() => !finished;
    }
  }
}
#endif