using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class ASync {
    static ASyncHelperBehaviour coroutineHelper(GameObject go) {
      return
        go.GetComponent<ASyncHelperBehaviour>() ??
        go.AddComponent<ASyncHelperBehaviour>();
    }

    static ASyncHelperBehaviour _behaviour;

    static ASyncHelperBehaviour behaviour { get {
      if (
/** 
 * (object)_behaviour == null fails when running in editor tests, because the behaviour gets destroyed
 * So we run !_behaviour in editor which works perfectly fine
**/
#if UNITY_EDITOR
        !_behaviour
#else
        // ReSharper disable once RedundantCast.0
        (object)_behaviour == null
#endif
      ) {
        const string name = "ASync Helper";
        var go = new GameObject(name);
        // Notice that DontDestroyOnLoad can only be used in play mode and, as such, cannot
        // be part of an editor script.
        if (Application.isPlaying) UnityEngine.Object.DontDestroyOnLoad(go);
        _behaviour = coroutineHelper(go);
      }
      return _behaviour;
    } }

    static ASync() { var _ = behaviour; }

    public static Future<A> StartCoroutine<A>(
      Func<Promise<A>, IEnumerator> coroutine
    ) {
      return Future<A>.async(p => behaviour.StartCoroutine(coroutine(p)));
    }

    public static Coroutine StartCoroutine(IEnumerator coroutine) {
      return new Coroutine(behaviour, coroutine);
    }

    public static Coroutine WithDelay(
      float seconds, Action action, 
      MonoBehaviour behaviour = null, TimeScale timeScale = TimeScale.Unity
    ) => WithDelay(Duration.fromSeconds(seconds), action, behaviour, timeScale);

    public static Coroutine WithDelay(
      Duration duration, Action action, 
      MonoBehaviour behaviour=null, TimeScale timeScale=TimeScale.Unity
    ) {
      behaviour = behaviour ?? ASync.behaviour;
      var enumerator = WithDelayEnumerator(duration, action, timeScale);
      return new Coroutine(behaviour, enumerator);
    }

    public static void OnMainThread(Action action, bool runNowIfOnMainThread = true) => 
      Threads.OnMainThread.run(action, runNowIfOnMainThread);

    public static Coroutine NextFrame(Action action) {
      return NextFrame(behaviour, action);
    }

    public static Coroutine NextFrame(GameObject gameObject, Action action) {
      return NextFrame(coroutineHelper(gameObject), action);
    }

    public static Coroutine NextFrame(MonoBehaviour behaviour, Action action) {
      var enumerator = NextFrameEnumerator(action);
      return new Coroutine(behaviour, enumerator);
    }

    public static Coroutine AfterXFrames(
      int framesToSkip, Action action
    ) { return AfterXFrames(behaviour, framesToSkip, action); }

    public static Coroutine AfterXFrames(
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

    public static void NextPostRender(Camera camera, Action action) {
      NextPostRender(camera, 1, action);
    }

    public static void NextPostRender(Camera camera, int afterFrames, Action action) {
      var pr = camera.gameObject.AddComponent<NextPostRenderBehaviour>();
      pr.init(action, afterFrames);
    }

    /* Do thing every frame until f returns false. */
    public static Coroutine EveryFrame(Fn<bool> f) {
      return EveryFrame(behaviour, f);
    }

    /* Do thing every frame until f returns false. */
    public static Coroutine EveryFrame(GameObject go, Fn<bool> f) {
      return EveryFrame(coroutineHelper(go), f);
    }

    /* Do thing every frame until f returns false. */
    public static Coroutine EveryFrame(MonoBehaviour behaviour, Fn<bool> f) {
      var enumerator = EveryWaitEnumerator(null, f);
      return new Coroutine(behaviour, enumerator);
    }

    /* Do thing every X seconds until f returns false. */
    public static Coroutine EveryXSeconds(float seconds, Fn<bool> f) {
      return EveryXSeconds(seconds, behaviour, f);
    }

    /* Do thing every X seconds until f returns false. */
    public static Coroutine EveryXSeconds(float seconds, GameObject go, Fn<bool> f) {
      return EveryXSeconds(seconds, coroutineHelper(go), f);
    }

    /* Do thing every X seconds until f returns false. */
    public static Coroutine EveryXSeconds(float seconds, MonoBehaviour behaviour, Fn<bool> f) {
      var enumerator = EveryWaitEnumerator(new WaitForSecondsRealtime(seconds), f);
      return new Coroutine(behaviour, enumerator);
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
    public static Cancellable<Future<Either<Cancelled, Either<WWWError, WWW>>>> wwwFuture(this WWW www) {
      Promise<Either<Cancelled, Either<WWWError, WWW>>> promise;
      var f = Future<Either<Cancelled, Either<WWWError, WWW>>>.async(out promise);

      var wwwCoroutine = StartCoroutine(WWWEnumerator(www, promise));

      return Cancellable.a(f, () => {
        if (www.isDone) return false;

        wwwCoroutine.stop();
        www.Dispose();
        promise.complete(new Either<Cancelled, Either<WWWError, WWW>>(Cancelled.instance));
        return true;
      });
    }

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
      Duration duration, Action action, TimeScale timeScale=TimeScale.Unity
    ) {
      var seconds = duration.seconds;
      if (timeScale == TimeScale.Unity) yield return new WaitForSeconds(seconds);
      else {
        var waitTime = timeScale.now() + seconds;
        while (waitTime > timeScale.now()) yield return null;
      }
      action();
    }

    public static IEnumerator NextFrameEnumerator(Action action) {
      yield return null;
      action();
    }

    public static IEnumerator EveryWaitEnumerator(IEnumerator wait, Fn<bool> f) {
      // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
      while (f()) yield return wait;
    }

    public static IObservable<bool> onAppPause => behaviour.onPause;

    public static IObservable<Unit> onAppQuit => behaviour.onQuit;

    public static IObservable<Unit> onLateUpdate { get; } = behaviour.onLateUpdate;

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
      this IEnumerable<A> enumerable, Fn<A, Future<B>> asyncAction
    ) {
      var rxRef = RxRef.a(F.none<B>());
      inAsyncSeq(enumerable.GetEnumerator(), rxRef, asyncAction);
      return rxRef;
    }

    static void inAsyncSeq<A, B>(
      IEnumerator<A> e, IRxRef<Option<B>> rxRef,
      Fn<A, Future<B>> asyncAction
    ) {
      if (! e.MoveNext()) return;
      asyncAction(e.Current).onComplete(b => {
        rxRef.value = F.some(b);
        inAsyncSeq(e, rxRef, asyncAction);
      });
    }
  }

  public class WaitForSecondsUnscaled : ReusableYieldInstruction {
    readonly float time;
    float waitTime;

    public WaitForSecondsUnscaled(float time) { this.time = time; }

    protected override void init() {
      waitTime = Time.unscaledTime + time;
    }

    public override bool keepWaiting => Time.unscaledTime < waitTime;
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

#if !UNITY_5_4_OR_NEWER
// Unity has this since 5.4
namespace UnityEngine {
  using com.tinylabproductions.TLPLib.Concurrent;

  public class WaitForSecondsRealtime : ReusableYieldInstruction {
    readonly float time;
    float finishTime;

    public WaitForSecondsRealtime(float time) { this.time = time; }

    protected override void init() {
      finishTime = Time.realtimeSinceStartup + time;
    }

    public override bool keepWaiting => Time.realtimeSinceStartup < finishTime;
  }
}
#endif
