using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Concurrent.unity_web_request;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine;
using UnityEngine.Networking;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class ASync {
    static ASyncHelperBehaviourEmpty coroutineHelper(GameObject go) =>
      go.GetComponent<ASyncHelperBehaviourEmpty>()
      ?? go.AddComponent<ASyncHelperBehaviourEmpty>();

    static ASyncHelperBehaviour _behaviour;

    [PublicAPI]
    public static ASyncHelperBehaviour behaviour { get {
      if (
#if !UNITY_EDITOR
        // Cast to System.Object here, to avoid Unity overloaded UnityEngine.Object == operator
        // which calls into native code to check whether objects are alive (which is a lot slower than
        // managed reference check).
        //
        // The only case where this should be uninitialized is until we create a reference on first access
        // in managed code.
        //
        // ReSharper disable once RedundantCast.0
        (object)_behaviour == null
#else
        // However...
        //
        // Managed reference check fails when running in editor tests, because the behaviour gets destroyed
        // for some reason, so we have to resort to unity checks in editor.
        !_behaviour
#endif
      ) {
        const string name = "ASync Helper";
        try {
          var go = new GameObject(name);
          // Notice that DontDestroyOnLoad can only be used in play mode and, as such, cannot
          // be part of an editor script.
          if (Application.isPlaying) UnityEngine.Object.DontDestroyOnLoad(go);
          _behaviour = go.EnsureComponent<ASyncHelperBehaviour>();
        }
        catch (Exception e) {
          Log.d.error(
            $"Failed to create {name}! Make sure {nameof(ASync)} is initialized from main thread!",
            e
          );
        }
      }
      return _behaviour;
    } }

    static ASync() { behaviour.forSideEffects(); }

    public static Future<A> StartCoroutine<A>(
      Func<Promise<A>, IEnumerator> coroutine
    ) => Future.async<A>(p => behaviour.StartCoroutine(coroutine(p)));

    public static ICoroutine StartCoroutine(IEnumerator coroutine) =>
      new UnityCoroutine(behaviour, coroutine);

    public static ICoroutine WithDelay(
      float seconds, Action action,
      MonoBehaviour behaviour = null, TimeScale timeScale = TimeScale.Unity
    ) => WithDelay(Duration.fromSeconds(seconds), action, behaviour, timeScale);

    public static ICoroutine WithDelay(
      Duration duration, Action action,
      MonoBehaviour behaviour = null, TimeScale timeScale = TimeScale.Unity
    ) => WithDelay(duration, action, timeScale.asContext(), behaviour);

    public static ICoroutine WithDelay(
      Duration duration, Action action, ITimeContext timeContext,
      MonoBehaviour behaviour = null
    ) {
      behaviour = behaviour ?? ASync.behaviour;
      var enumerator = WithDelayEnumerator(duration, action, timeContext);
      return new UnityCoroutine(behaviour, enumerator);
    }

    public static void OnMainThread(Action action, bool runNowIfOnMainThread = true) =>
      Threads.OnMainThread.run(action, runNowIfOnMainThread);

    public static ICoroutine NextFrame(Action action) => NextFrame(behaviour, action);

    public static ICoroutine NextFrame(GameObject gameObject, Action action) =>
      NextFrame(coroutineHelper(gameObject), action);

    public static ICoroutine NextFrame(MonoBehaviour behaviour, Action action) {
      var enumerator = NextFrameEnumerator(action);
      return new UnityCoroutine(behaviour, enumerator);
    }

    public static ICoroutine AfterXFrames(
      int framesToSkip, Action action
    ) => AfterXFrames(behaviour, framesToSkip, action);

    public static ICoroutine AfterXFrames(
      MonoBehaviour behaviour, int framesToSkip, Action action
    ) {
      return EveryFrame(behaviour, () => {
        if (framesToSkip <= 0) {
          action();
          return false;
        }
        else {
          framesToSkip--;
          return true;
        }
      });
    }

    public static void NextPostRender(Camera camera, Action action) => NextPostRender(camera, 1, action);

    public static void NextPostRender(Camera camera, int afterFrames, Action action) {
      var pr = camera.gameObject.AddComponent<NextPostRenderBehaviour>();
      pr.init(action, afterFrames);
    }

    /* Do thing every frame until f returns false. */
    public static ICoroutine EveryFrame(Func<bool> f) => EveryFrame(behaviour, f);

    /* Do thing every frame until f returns false. */
    public static ICoroutine EveryFrame(GameObject go, Func<bool> f) => EveryFrame(coroutineHelper(go), f);

    /* Do thing every frame until f returns false. */
    public static ICoroutine EveryFrame(MonoBehaviour behaviour, Func<bool> f) {
      var enumerator = EveryWaitEnumerator(null, f);
      return new UnityCoroutine(behaviour, enumerator);
    }

    /* Do thing every X seconds until f returns false. */
    public static ICoroutine EveryXSeconds(float seconds, Func<bool> f) => EveryXSeconds(seconds, behaviour, f);

    /* Do thing every X seconds until f returns false. */
    public static ICoroutine EveryXSeconds(float seconds, GameObject go, Func<bool> f) =>
      EveryXSeconds(seconds, coroutineHelper(go), f);

    /* Do thing every X seconds until f returns false. */
    public static ICoroutine EveryXSeconds(float seconds, MonoBehaviour behaviour, Func<bool> f) {
      var enumerator = EveryWaitEnumerator(new WaitForSecondsRealtimeReusable(seconds), f);
      return new UnityCoroutine(behaviour, enumerator);
    }

    /* Returns action that cancels our delayed call. */
    public static Action WithDelayFixedUpdate(GameObject go, float delay, Action act) {
      // TODO: probably this needs to be rewritten to use only one global component for fixed update
      if (delay < 1e-6) {
        // if delay is 0 call immediately
        // this is because we don't want to wait a single fixed update
        act();
        return () => { };
      }
      else {
        var component = go.AddComponent<ASyncFixedUpdateHelperBehaviour>();
        component.init(delay, act);
        return () => {
          if (component) UnityEngine.Object.Destroy(component);
        };
      }
    }

    /* Do async cancellable WWW request. */
    public static Cancellable<Future<Either<Cancelled, Either<WWWError, WWW>>>> toFuture(this WWW www) {
      Promise<Either<Cancelled, Either<WWWError, WWW>>> promise;
      var f = Future.async(out promise);

      var wwwCoroutine = StartCoroutine(WWWEnumerator(www, promise));

      return Cancellable.a(f, () => {
        if (www.isDone) return false;

        wwwCoroutine.stop();
        www.Dispose();
        promise.complete(new Either<Cancelled, Either<WWWError, WWW>>(Cancelled.instance));
        return true;
      });
    }

    /// <summary>Turn this request to future. Automatically cleans up the request.</summary>
    [PublicAPI]
    public static Future<Either<WebRequestError, A>> toFuture<A>(
      this UnityWebRequest req, AcceptedResponseCodes acceptedResponseCodes, 
      Func<UnityWebRequest, A> onSuccess
    ) {
      var f = Future.async<Either<WebRequestError, A>>(out var promise);
      var op = req.SendWebRequest();
      op.completed += operation => {
        var responseCode = req.responseCode;
        if (
#if UNITY_2018_2_OR_NEWER
          req.isNetworkError
#else
          req.isError
#endif
        ) {
          var msg = $"error: {req.error}, response code: {responseCode}";
          var url = new Url(req.url);
          promise.complete(
            responseCode == 0 && req.error == "Unknown Error"
              ? new WebRequestError(url, new NoInternetError(msg))
              : new WebRequestError(url, LogEntry.simple(msg))
          );
          req.Dispose();
        }
        else if (!acceptedResponseCodes.contains(responseCode)) {
          var url = new Url(req.url); // Capture URL before disposing
          var extrasB = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>();
          foreach (var header in req.GetResponseHeaders()) {
            extrasB.Add(F.kv(header.Key, header.Value));
          }
          extrasB.Add(F.kv("response-text", req.downloadHandler.text));
          req.Dispose();
          promise.complete(new WebRequestError(url, LogEntry.extras_(
            $"Received response code {responseCode} was not in {acceptedResponseCodes}",
            extrasB.MoveToImmutableSafe()
          )));
        }
        else {
          var a = onSuccess(req);
          req.Dispose();
          promise.complete(a);
        }
      };
      return f;
    }

    [PublicAPI]
    public static Future<Either<LogEntry, A>> toFutureSimple<A>(
      this UnityWebRequest req, AcceptedResponseCodes acceptedResponseCodes, Func<UnityWebRequest, A> onSuccess
    ) => req.toFuture(acceptedResponseCodes, onSuccess).map(_ => _.mapLeft(err => err.simplify));

    [PublicAPI]
    public static Cancellable<Future<Either<Cancelled, Either<WWWError, Texture2D>>>> asTexture(
      this Cancellable<Future<Either<Cancelled, Either<WWWError, WWW>>>> cancellable
    ) => cancellable.map(f => f.map(e => e.mapRight(_ => _.asTexture())));

    public static IEnumerator WWWEnumerator(WWW www) { yield return www; }

    public static IEnumerator WWWEnumerator(WWW www, Promise<Either<Cancelled, Either<WWWError, WWW>>> promise) =>
      WWWEnumerator(www).afterThis(() => promise.complete(
        Either<Cancelled, Either<WWWError, WWW>>.Right(www.toEither())
      ));

    /* Wait until enumerator is completed and then do action */
    public static IEnumerator afterThis(this IEnumerator enumerator, Action action) {
      while (enumerator.MoveNext()) yield return enumerator.Current;
      action();
    }

    public static IEnumerator WithDelayEnumerator(
      Duration duration, Action action, ITimeContext timeContext
    ) {
      if (timeContext == TimeContext.playMode) {
        // WaitForSeconds is optimized Unity in native code
        // waiters that extend CustomYieldInstruction (eg. WaitForSecondsRealtime) call C# code every frame,
        // so we don't need special handling for them
        yield return new WaitForSeconds(duration.seconds);
      }
      else {
        var waiter = timeContext == TimeContext.fixedTime ? CoroutineUtils.waitFixed : null;
        var waitTime = timeContext.passedSinceStartup + duration.toTimeSpan;
        while (waitTime > timeContext.passedSinceStartup) yield return waiter;
      }
      action();
    }

    /// <summary>Runs action forever every frame.</summary>
    [PublicAPI]
    public static IEnumerator everyFrameEnumerator(Action action) {
      while (true) {
        action();
        yield return null;
      }
      // ReSharper disable once IteratorNeverReturns
    }
    
    public static IEnumerator NextFrameEnumerator(Action action) {
      yield return null;
      action();
    }

    public static IEnumerator EveryWaitEnumerator(IEnumerator wait, Func<bool> f) {
      // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
      while (f()) yield return wait;
    }

    public static IRxObservable<bool> onAppPause => behaviour.onPause;

    public static IRxObservable<Unit> onAppQuit => behaviour.onQuit;

    public static IRxObservable<Unit> onLateUpdate { get; } = behaviour.onLateUpdate;

    /**
     * Takes a function that transforms an element into a future and
     * applies it to all elements in given sequence.
     *
     * However instead of applying all elements concurrently it waits
     * for the future from previous element to complete before applying
     * the next element.
     *
     * Returns reactive value that can be used to observe current stage
     * of the application.
     **/
    public static IRxVal<Option<B>> inAsyncSeq<A, B>(
      this IEnumerable<A> enumerable, Func<A, Future<B>> asyncAction
    ) {
      var rxRef = RxRef.a(F.none<B>());
      inAsyncSeq(enumerable.GetEnumerator(), rxRef, asyncAction);
      return rxRef;
    }

    static void inAsyncSeq<A, B>(
      IEnumerator<A> e, IRxRef<Option<B>> rxRef,
      Func<A, Future<B>> asyncAction
    ) {
      if (! e.MoveNext()) return;
      asyncAction(e.Current).onComplete(b => {
        rxRef.value = F.some(b);
        inAsyncSeq(e, rxRef, asyncAction);
      });
    }

    /// <summary>
    /// Split running action over collection over N chunks separated by a given yield instruction.
    /// </summary>
    [PublicAPI] public static IEnumerator overNYieldInstructions<A>(
      ICollection<A> collection, int n, Action<A, int> onA, YieldInstruction instruction = null
    ) {
      var chunkSize = collection.Count / n;
      var idx = 0;
      foreach (var a in collection) {
        onA(a, idx);
        if (idx % chunkSize == 0) yield return instruction;
        idx++;
      }
    }
  }

  public class WaitForSecondsUnscaled : ReusableYieldInstruction {
    readonly float time;
    float waitTime;

    public WaitForSecondsUnscaled(float time) { this.time = time; }

    protected override void init() => waitTime = Time.unscaledTime + time;

    public override bool keepWaiting => Time.unscaledTime < waitTime;
  }

  /** WaitForSecondsRealtime from Unity is not reusable. */
  public class WaitForSecondsRealtimeReusable : ReusableYieldInstruction {
    readonly float time;
    float finishTime;

    public WaitForSecondsRealtimeReusable(float time) { this.time = time; }

    protected override void init() => finishTime = Time.realtimeSinceStartup + time;

    public override bool keepWaiting => Time.realtimeSinceStartup < finishTime;
  }

  // If we extend YieldInstruction we can not reuse its instances
  // because it inits end condition only in constructor.
  // We can reuse instances of ReusableYieldInstruction but we can't
  // use the same instance in multiple places at once
  public abstract class ReusableYieldInstruction : IEnumerator {
    bool inited;

    protected abstract void init();

    public abstract bool keepWaiting { get; }

    public bool MoveNext() {
      if (!inited) {
        init();
        inited = true;
      }
      var result = keepWaiting;
      if (!result) inited = false;
      return result;
    }

    // Never gets called
    public void Reset() {}

    // https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html
    public object Current => null;
  }
}