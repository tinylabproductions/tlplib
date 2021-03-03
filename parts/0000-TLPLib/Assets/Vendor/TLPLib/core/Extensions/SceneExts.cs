using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.scenes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class SceneExts {
    // includes inactive objects
    // may not work on scene awake
    // http://forum.unity3d.com/threads/bug-getrootgameobjects-is-not-working-in-awake.379317/
    public static IEnumerable<T> findObjectsOfTypeAll<T>(
      this Scene scene, bool includeInactive = true
    ) where T : Component =>
      scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<T>(includeInactive));

    public static IEnumerable<T> findObjectsOfTypeAll<T>(
      this IEnumerable<Scene> scenes, bool includeInactive = true
    ) where T : Component => 
      scenes.SelectMany(scene => scene.findObjectsOfTypeAll<T>(includeInactive));

    /// <summary>
    /// Retrieve first <see cref="A"/> attached to a root <see cref="GameObject"/> in the <see cref="Scene"/>.
    /// </summary>
    public static Either<ErrorMsg, A> findComponentOnRootGameObjects<A>(this Scene scene) where A : Component =>
      scene.GetRootGameObjects()
      .collectFirst(go => go.GetComponent<A>().opt())
      .toRight(() => new ErrorMsg($"Couldn't find {typeof(A)} in scene '{scene.path}' root game objects"));

    public static ScenePath scenePath(this Scene scene) => new ScenePath(scene.path);
  }
}
