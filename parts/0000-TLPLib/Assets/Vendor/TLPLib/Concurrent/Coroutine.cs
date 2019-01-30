using System;
using System.Collections;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

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

    public UnityCoroutine(
      MonoBehaviour behaviour, IEnumerator enumerator,
      [CallerFilePath] string callerFilePath = "",
      [CallerMemberName] string callerMemberName = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      var fixBugsEnumerator = fixUnityBugs(enumerator);
      if (Application.isPlaying) {
        behaviour.StartCoroutine(fixBugsEnumerator);
      } 
#if UNITY_EDITOR
      else {
        // This is a hack to run coroutine in edit mode, no other yield instructions
        // beside null are supported.
        void updateFn() {
          // ReSharper disable once DelegateSubtraction
          void unsubscribe() => UnityEditor.EditorApplication.update -= updateFn;
          
          var hasNext = fixBugsEnumerator.MoveNext();
          if (!behaviour || !hasNext) unsubscribe();
          if (hasNext) {
            var yieldInstruction = fixBugsEnumerator.Current;
            if (yieldInstruction != null) {
              unsubscribe();
              Log.d.error(
                $"Aborting coroutine started in {callerMemberName} @ {callerFilePath}:{callerLineNumber}, " +
                $"because it yielded {yieldInstruction}, which we do not know how to fake to in editor!"
              );
            }
          }
        }
        UnityEditor.EditorApplication.update += updateFn;
      }
#endif
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
