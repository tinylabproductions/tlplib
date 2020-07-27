using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using Smooth.Pools;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface Coroutine : IDisposable, IEnumerator {
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

  [PublicAPI] public static class CoroutineExts {
    public static void stop(this Coroutine c) => c.Dispose();
    
    public static AggregateCoroutine aggregate(this Coroutine[] coroutines) => 
      new AggregateCoroutine(coroutines);
    public static AggregateCoroutine aggregate(this IEnumerable<Coroutine> coroutines) => 
      new AggregateCoroutine(coroutines.ToArray());
  }

  public static class CoroutineUtils {
    public static readonly YieldInstruction waitFixed = new WaitForFixedUpdate();
  }

  public sealed class UnityCoroutine : CustomYieldInstruction, Coroutine {
    public event Action onFinish;
    public bool finished { get; private set; }
    public override bool keepWaiting => !finished;

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

    static readonly Pool<Stack<IEnumerator>> stackPool = new Pool<Stack<IEnumerator>>(
      () => new Stack<IEnumerator>(),
      s => s.Clear()
    );
    
    /**
     * So...
     *
     * 1. https://fogbugz.unity3d.com/default.asp?826400_tcbicqltkckqmer1
     * 2. Unity API has no way to check whether Coroutine has been completed.
     **/
    IEnumerator fixUnityBugs(IEnumerator startingEnumerator) {
      using (var stackDisposable = stackPool.BorrowDisposable()) {
        var stack = stackDisposable.value;
        stack.Push(startingEnumerator);
        
        while (stack.Count > 0 && !shouldStop) {
          var enumerator = stack.Peek();
          if (enumerator.MoveNext()) {
            var current = enumerator.Current;
            switch (current) {
              case IEnumerator innerEnumerator:
                stack.Push(innerEnumerator);
                break;
              default:
                switch (current) {
                  case null:
                  case YieldInstruction _:
                    break;
                  default:
                    if (Log.d.isDebug()) Log.d.warn(
                      $"{stack.Count}: {enumerator} yielding unknown {current.GetType()}"
                    );
                    break;
                }
                yield return current;
                break;
            }
          }
          else {
            stack.Pop();
          }
        }

        finished = true;
        onFinish?.Invoke();
      }
    }

    public void stop() => shouldStop = true;

    public void Dispose() => stop();
  }

  public sealed class AggregateCoroutine : CustomYieldInstruction, Coroutine {
    readonly Coroutine[] coroutines;
    int finishedCoroutines;
    
    public event Action onFinish;
    public bool finished { get; private set; }
    public override bool keepWaiting => !finished;

    public AggregateCoroutine(Coroutine[] coroutines) { 
      this.coroutines = coroutines;
      foreach (var c in coroutines) {
        if (c.finished) coroutineFinished();
        else c.onFinish += coroutineFinished;
      }

      void coroutineFinished() {
        finishedCoroutines++;
        if (finishedCoroutines == coroutines.Length) {
          onFinish?.Invoke();
          finished = true;
        }
      }
    }

    public void Dispose() {
      foreach (var coroutine in coroutines) {
        coroutine.Dispose();
      }
    }
  } 
}
