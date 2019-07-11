using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using Smooth.Dispose;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Pools {
  public static class GameObjectPool {
    public static class Init {
      [PublicAPI]
      public static Init<T> withReparenting<T>(
        string name, Func<T> create,
        Action<T> wakeUp = null, Action<T> sleep = null,
        bool dontDestroyOnLoad = true, Transform parent = null
      ) => Init<T>.withReparenting(name, create, wakeUp, sleep, dontDestroyOnLoad, parent);

      [PublicAPI]
      public static Init<T> noReparenting<T>(
        string name, Func<T> create, bool dontDestroyOnLoad,
        Action<T> wakeUp = null, Action<T> sleep = null
      ) => Init<T>.noReparenting(name, create, wakeUp, sleep, dontDestroyOnLoad);
    }
    
    public struct Init<T> {
      public readonly string name;
      public readonly Func<T> create;
      public readonly Option<Action<T>> wakeUp, sleep;
      public readonly bool dontDestroyOnLoad;

      // Some: parent transform for GameObjectPool. (null = root)
      // None: no reparenting, gameobjects are only disabled on release.
      public readonly Option<Transform> parent;

      Init(
        string name, Func<T> create, Option<Transform> parent,
        Action<T> wakeUp = null, Action<T> sleep = null,
        bool dontDestroyOnLoad = true
      ) {
        this.name = name;
        this.create = create;
        this.wakeUp = wakeUp.opt();
        this.sleep = sleep.opt();
        this.dontDestroyOnLoad = dontDestroyOnLoad;
        this.parent = parent;
      }

      [PublicAPI]
      public static Init<T> withReparenting(
        string name, Func<T> create,
        Action<T> wakeUp = null, Action<T> sleep = null,
        bool dontDestroyOnLoad = true, Transform parent = null
      ) => new Init<T>(
        name, create, parent.some(), wakeUp, sleep, dontDestroyOnLoad
      );

      [PublicAPI]
      public static Init<T> noReparenting(
        string name, Func<T> create,
        Action<T> wakeUp = null, Action<T> sleep = null,
        bool dontDestroyOnLoad = true
      ) => new Init<T>(
        name, create, Option<Transform>.None, wakeUp, sleep, dontDestroyOnLoad
      );
    }

    public static GameObjectPool<T> a<T>(
      Init<T> init, Func<T, GameObject> toGameObject
    ) => new GameObjectPool<T>(init, toGameObject);
    
    public static GameObjectPool<GameObject> a(
      Init<GameObject> init
    ) => new GameObjectPool<GameObject>(init, _ => _);

    public static GameObjectPool<A> a<A>(Init<A> init, int initialSize = 0) where A : Component =>
      new GameObjectPool<A>(init, initialSize: initialSize, toGameObject: a => {
        if (!a) Log.d.error(
          $"Component {typeof(A)} is destroyed in {nameof(GameObjectPool)} '{init.name}'!"
        ); 
        return a.gameObject;
      });
  }

  public class GameObjectPool<T> {
    readonly Stack<T> values;
    readonly Option<Transform> rootOpt;

    readonly Func<T, GameObject> toGameObject;
    readonly Func<T> create;
    readonly Option<Action<T>> wakeUp, sleep;
    readonly bool dontDestroyOnLoad;

    public GameObjectPool(GameObjectPool.Init<T> init, Func<T, GameObject> toGameObject, int initialSize = 0) {
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
      dontDestroyOnLoad = init.dontDestroyOnLoad;
      values = initialSize == 0 ? new Stack<T>() : new Stack<T>(initialSize);

      for (var i = 0; i < initialSize; i++) {
        release(createAndInit());
      }
    }

    T createAndInit() {
      var result = create();
      var go = toGameObject(result);
      var t = go.transform;
      if (dontDestroyOnLoad && !t.parent) Object.DontDestroyOnLoad(go);
      return result;
    }

    public T borrow() {
      var result = values.Count > 0 ? values.Pop() : createAndInit();
      var go = toGameObject(result);
      var t = go.transform;
      t.localPosition = Vector3.zero;
      t.rotation = Quaternion.identity;
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

    public void dispose(Action<T> disposeFn) {
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