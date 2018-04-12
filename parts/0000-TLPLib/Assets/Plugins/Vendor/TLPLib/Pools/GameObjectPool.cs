using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using Smooth.Dispose;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Pools {
  public static class GameObjectPool {
    public struct Init<T> {
      public readonly string name;
      public readonly Fn<T> create;
      public readonly Option<Act<T>> wakeUp, sleep;
      public readonly bool dontDestroyOnLoad;

      // Some: parent transform for GameObjectPool. (null = root)
      // None: no reparenting, gameobjects are only disabled on release.
      public readonly Option<Transform> parent;

      Init(
        string name, Fn<T> create, Option<Transform> parent,
        Act<T> wakeUp = null, Act<T> sleep = null,
        bool dontDestroyOnLoad = true
      ) {
        this.name = name;
        this.create = create;
        this.wakeUp = wakeUp.opt();
        this.sleep = sleep.opt();
        this.dontDestroyOnLoad = dontDestroyOnLoad;
        this.parent = parent;
      }

      public static Init<T> withReparenting(
        string name, Fn<T> create,
        Act<T> wakeUp = null, Act<T> sleep = null,
        bool dontDestroyOnLoad = true, Transform parent = null
      ) => new Init<T>(
        name, create, parent.some(), wakeUp, sleep, dontDestroyOnLoad
      );

      public static Init<T> noReparenting(
        string name, Fn<T> create,
        Act<T> wakeUp = null, Act<T> sleep = null,
        bool dontDestroyOnLoad = true
      ) => new Init<T>(
        name, create, Option<Transform>.None, wakeUp, sleep, dontDestroyOnLoad
      );
    }

    public static GameObjectPool<T> a<T>(
      Init<T> init, Fn<T, GameObject> toGameObject
    ) => new GameObjectPool<T>(init, toGameObject);
    
    public static GameObjectPool<GameObject> a(
      Init<GameObject> init
    ) => new GameObjectPool<GameObject>(init, _ => _);

    public static GameObjectPool<A> a<A>(Init<A> init) where A : Component =>
      new GameObjectPool<A>(init, a => {
        if (!a) Log.d.error(
          $"Component {typeof(A)} is destroyed in {nameof(GameObjectPool)} '{init.name}'!"
        ); 
        return a.gameObject;
      });
  }

  public class GameObjectPool<T> {
    readonly Stack<T> values = new Stack<T>();
    readonly Option<Transform> rootOpt;

    readonly Fn<T, GameObject> toGameObject;
    readonly Fn<T> create;
    readonly Option<Act<T>> wakeUp, sleep;

    public GameObjectPool(GameObjectPool.Init<T> init, Fn<T, GameObject> toGameObject) {
      rootOpt = init.parent.map(parent => {
        var rootParent = new GameObject($"{nameof(GameObjectPool)}: {init.name}").transform;
        rootParent.parent = parent;
        if (init.dontDestroyOnLoad) Object.DontDestroyOnLoad(rootParent.gameObject);
        return rootParent;
      });

      this.toGameObject = toGameObject;
      create = init.create;
      wakeUp = init.wakeUp;
      sleep = init.sleep;
    }

    public T borrow() {
      var result = values.Count > 0 ? values.Pop() : create();
      var go = toGameObject(result);
      go.transform.localPosition = Vector3.zero;
      go.transform.rotation = Quaternion.identity;
      go.SetActive(true);
      if (wakeUp.isSome) wakeUp.get(result);
      return result;
    }

    public void release(T value) {
      if (sleep.isSome) sleep.get(value);
      var go = toGameObject(value);
      foreach (var root in rootOpt) {
        go.transform.SetParent(root, false);
      }
      go.SetActive(false);
      values.Push(value);
    }

    public void dispose(Act<T> disposeFn) {
      foreach (var value in values) {
        disposeFn(value);
      }

      foreach (var root in rootOpt) {
        Object.Destroy(root.gameObject);
      }

      values.Clear();
    }

    public Disposable<T> BorrowDisposable() => Disposable<T>.Borrow(borrow(), release);
  }
}