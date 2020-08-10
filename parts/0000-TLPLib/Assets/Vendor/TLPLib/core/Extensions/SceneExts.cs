﻿using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class SceneExts {
    // includes inactive objects
    // may not work on scene awake
    // http://forum.unity3d.com/threads/bug-getrootgameobjects-is-not-working-in-awake.379317/
    [PublicAPI]
    public static IEnumerable<T> findObjectsOfTypeAll<T>(this Scene scene, bool includeInactive = true) where T : Object {
      return scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<T>(includeInactive));
    }
    
    [PublicAPI]
    public static IEnumerable<T> findObjectsOfTypeAll<T>(this IEnumerable<Scene> scenes, bool includeInactive = true) where T : Object {
      return scenes.SelectMany(scene => scene.GetRootGameObjects()).SelectMany(o => o.GetComponentsInChildren<T>(includeInactive));
    }

    /// <summary>
    /// Retrieve first <see cref="A"/> attached to a root <see cref="GameObject"/> in the <see cref="Scene"/>.
    /// </summary>
    [PublicAPI]
    public static Either<ErrorMsg, A> findComponentOnRootGameObjects<A>(this Scene scene) where A : Component =>
      scene.GetRootGameObjects()
      .collectFirst(go => go.GetComponent<A>().opt())
      .fold(
        () => F.left<ErrorMsg, A>(new ErrorMsg(
          $"Couldn't find {typeof(A)} in scene '{scene.path}' root game objects"
        )),
        F.right<ErrorMsg, A>
      );

    [PublicAPI]
    public static ScenePath scenePath(this Scene scene) => new ScenePath(scene.path);
  }
}
