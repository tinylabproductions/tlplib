#if UNITY_EDITOR
using System;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.exts;
using pzd.lib.log;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [Singleton, PublicAPI] public sealed partial class EditorTimeContext : ITimeContext {
    [LazyProperty] static ILog log => Log.d.withScope(nameof(EditorTimeContext));
    
    public TimeSpan passedSinceStartup => TimeSpan.FromSeconds(EditorApplication.timeSinceStartup);
    
    public ICoroutine after(TimeSpan duration, Action act, string name = null) => 
      new EditorCoroutine(duration, act, name ?? "unnamed");

    class EditorCoroutine : ICoroutine {
      public event CoroutineFinishedOrStopped onFinish;
      public CoroutineState state { get; private set; } = CoroutineState.Running;
      
      readonly TimeSpan duration;
      readonly Action action;
      readonly double startedAt;
      readonly string name;

      double scheduledAt => startedAt + duration.TotalSeconds;
      
      public EditorCoroutine(TimeSpan duration, Action action, string name) {
        this.duration = duration;
        this.action = action;
        this.name = name;
        startedAt = EditorApplication.timeSinceStartup;
        log.mDebug($"Scheduling '{name}' at {scheduledAt}, {startedAt.echo()}");
        
        EditorApplication.update += onUpdate;
      }

      void onUpdate() {
        var now = EditorApplication.timeSinceStartup;
        if (now >= scheduledAt) {
          state = CoroutineState.Finished;
          log.mDebug($"Running '{name}' at {now.echo()}, {scheduledAt.echo()}, {startedAt.echo()}");
          action();
          onFinish?.Invoke(finished: true);
          onFinish = null;
          dispose(doLog: false);
        }
      }

      public void Dispose() {
        if (!state.isRunning()) return;
        dispose(doLog: true);
      }

      void dispose(bool doLog) {
        if (doLog && log.isDebug()) log.debug($"Disposing '{name}' scheduled at {scheduledAt}, {startedAt.echo()}");
        EditorApplication.update -= onUpdate;
        state = CoroutineState.Stopped;
        onFinish?.Invoke(finished: false);
        onFinish = null;
      }

      public void Reset() {}
      public object Current => null;
      public bool MoveNext() => state.isRunning();
    }
  }
}
#endif