using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.dispose;
using pzd.lib.log;
using Log = com.tinylabproductions.TLPLib.Logger.Log;

namespace com.tinylabproductions.TLPLib.Dispose {
  [PublicAPI] public static class DisposableTrackerU {
    [LazyProperty, Implicit] static ILog log => Log.d.withScope(nameof(DisposableTrackerU));

    /// <summary>
    /// Use this in methods with RuntimeInitializeOnLoadMethod instead of NoOpDisposableTracker to dispose properly
    /// in editor
    /// </summary>
    [LazyProperty] public static IDisposableTracker disposeOnExitPlayMode => new DisposableTracker(log);
    
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void init() {
      UnityEditor.EditorApplication.playModeStateChanged += change => {
        if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
          disposeOnExitPlayMode.Dispose();
        }
      };
    }
#endif
  }
}