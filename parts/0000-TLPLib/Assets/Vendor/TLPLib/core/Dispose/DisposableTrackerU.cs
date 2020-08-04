using JetBrains.Annotations;
using pzd.lib.dispose;

namespace com.tinylabproductions.TLPLib.Dispose {
  [PublicAPI] public static class DisposableTrackerU {
    /// <summary>
    /// Use this in methods with RuntimeInitializeOnLoadMethod instead of NoOpDisposableTracker to dispose properly
    /// in editor
    /// </summary>
    public static IDisposableTracker disposeOnExitPlayMode = new DisposableTracker();
    
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