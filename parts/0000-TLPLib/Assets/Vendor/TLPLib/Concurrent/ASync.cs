using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class ASync {
    private static ASyncHelperBehaviour coroutineHelper(GameObject go) {
      return 
        go.GetComponent<ASyncHelperBehaviour>() ?? 
        go.AddComponent<ASyncHelperBehaviour>();
    }

    private static ASyncHelperBehaviour _behaviour;

    private static ASyncHelperBehaviour behaviour { get {
      if (((object)_behaviour) == null) { 
        const string name = "ASync Helper";
        var go = new GameObject(name);
        // Notice that DontDestroyOnLoad can only be used in play mode and, as such, cannot
        // be part of an editor script.
        if (Application.isPlaying) UnityEngine.Object.DontDestroyOnLoad(go);
        _behaviour = coroutineHelper(go);
      }
      return _behaviour;
    } }

    public static void init() { var init = behaviour; }

    public static Future<A> StartCoroutine<A>(
      Func<Promise<A>, IEnumerator> coroutine
    ) {
      return Future<A>.async(p => behaviour.StartCoroutine(coroutine(p)));
    }

    public static Coroutine StartCoroutine(IEnumerator coroutine) {
      return new Coroutine(behaviour, coroutine);
    }

    [Obsolete("Use Coroutine#stop")]
    public static void StopCoroutine(IEnumerator enumerator) {
      behaviour.StopCoroutine(enumerator);
    }

    [Obsolete("Use Coroutine#stop")]
    public static void StopCoroutine(Coroutine coroutine) {
      coroutine.stop();
    }

    public static Coroutine WithDelay(float seconds, Act action) {
      return WithDelay(seconds, behaviour, action);
    }

    public static Coroutine WithDelay(
      float seconds, MonoBehaviour behaviour, Act action
    ) {
      var enumerator = WithDelayEnumerator(seconds, action);
      return new Coroutine(behaviour, enumerator);
    }

    public static void OnMainThread(Act action) { Threads.OnMainThread.run(action);}

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

    public static void NextPostRender(Camera camera, Act action) {
      NextPostRender(camera, 1, action);
    }

    public static void NextPostRender(Camera camera, int afterFrames, Act action) {
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
      var enumerator = EveryWaitEnumerator(new WaitForSeconds(seconds), f);
      return new Coroutine(behaviour, enumerator);
    }

    /* Returns action that cancels our delayed call. */
    public static Act WithDelayFixedUpdate(GameObject go, float delay, Act act) {
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

    /* Do async WWW request. */
    public static Future<Either<WWWError, WWW>> wwwFuture(this WWW www) {
      return Future<Either<WWWError, WWW>>.async(p => StartCoroutine(WWWEnumerator(www, p)));
    }

    static readonly ASyncOneAtATimeQueue<Fn<WWW>, Either<WWWError, WWW>> wwwsQueue = 
      new ASyncOneAtATimeQueue<Fn<WWW>, Either<WWWError, WWW>>(
        "wwwsQueue", 
        (createWWW, promise) => StartCoroutine(WWWEnumerator(createWWW(), promise))
      );

    /* Do async WWW request, but only do one WWW request at a time - there was an IL2CPP bug where
       having several WWWs executing at once crashed the runtime. */
    public static Future<Either<WWWError, WWW>> oneAtATimeWWW(Fn<WWW> createWWW) {
      return wwwsQueue.query(createWWW);
    }

    public static IEnumerator WWWEnumerator(WWW www) {
      yield return www;
    }

    /* Wait until enumerator is completed and then do action */
    public static IEnumerator afterThis(this IEnumerator enumerator, Act action) {
      while (enumerator.MoveNext()) yield return enumerator.Current;
      action();
    }

    public static IEnumerator WWWEnumerator(WWW www, Promise<Either<WWWError, WWW>> promise) {
      return WWWEnumerator(www).afterThis(() => promise.complete(www.toEither()));
    }

    public static IEnumerator WithDelayEnumerator(
      float seconds, Act action
    ) {
      yield return new WaitForSeconds(seconds);
      action();
    }

    public static IEnumerator NextFrameEnumerator(Action action) {
      yield return null;
      action();
    }

    public static IEnumerator EveryWaitEnumerator(WaitForSeconds wait, Fn<bool> f) {
      // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
      while (f()) yield return wait;
    }

    public static IObservable<bool> onAppPause => behaviour.onPause;

    public static IObservable<Unit> onAppQuit => behaviour.onQuit;

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
}
