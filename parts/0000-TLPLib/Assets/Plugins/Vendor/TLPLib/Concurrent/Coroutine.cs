using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface Coroutine : IDisposable {
    /**
     * We could use Future here, but future is a heap allocated object and
     * we don't want each coroutine to allocate 2 extra heap objects.
     *
     * So instead we use event + property.
     */
    event Action onFinish;
    /* false if coroutine is running, true if it completed or was stopped. */
    bool finished { get; }
  }

  public static class CoroutineExts {
    public static void stop(this Coroutine c) => c.Dispose();
  }

  public static class CoroutineUtils {
    public static readonly YieldInstruction waitFixed = new WaitForFixedUpdate();
  }

  public class UnityCoroutine : Coroutine {
    public event Action onFinish;
    public bool finished { get; private set; }

    bool shouldStop;

    public UnityCoroutine(MonoBehaviour behaviour, IEnumerator enumerator) {
      var fixBugsEnumerator = fixUnityBugs(enumerator);
      if (Application.isPlaying) {
        behaviour.StartCoroutine(fixBugsEnumerator);
      } else {
        #if UNITY_EDITOR
        // This is a hack to run coroutine in edit mode, yield is ignored.
        void updateFn() {
          // ReSharper disable once DelegateSubtraction
          if (!behaviour || !fixBugsEnumerator.MoveNext()) EditorApplication.update -= updateFn;
        }
        EditorApplication.update += updateFn;
        #endif
      }
    }

    /**
     * So...
     *
     * 1. https://fogbugz.unity3d.com/default.asp?826400_tcbicqltkckqmer1
     * 2. Unity API has no way to check whether Coroutine has been completed.
     **/
    IEnumerator fixUnityBugs(IEnumerator enumerator) {
      while (enumerator.MoveNext()) {
        yield return enumerator.Current;
        if (shouldStop) break;
      }

      finished = true;
      onFinish?.Invoke();
    }

    public void stop() => shouldStop = true;

    public void Dispose() => stop();
  }
}
